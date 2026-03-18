using ERP.Data;
using ERP.Entity;
using ERP.Entity.DTO;
using ERP.Entity.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductionController : ControllerBase
{
    private readonly ManufacturingDbContext _context;

    public ProductionController(ManufacturingDbContext context)
    {
        _context = context;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderDto dto)
    {
        if (dto.ProductId == Guid.Empty)
            return BadRequest("ProductId is required.");

        if (dto.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        // Check product exists
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            return NotFound("Product not found.");

        // Get BOM header for this product
        var bom = await _context.BillOfMaterials
            .Include(b => b.Items) // Include BOM items
            .FirstOrDefaultAsync(b => b.ProductId == dto.ProductId);

        if (bom == null || bom.Items == null || !bom.Items.Any())
            return BadRequest("No BOM defined for this product.");

        // Begin transaction for atomic operation
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create production order
            var order = new ProductionOrder
            {
                OrderNumber = $"PO-{DateTime.UtcNow.Ticks}",
                ProductId = dto.ProductId,
                BillOfMaterialId = bom.Id,
                PlannedQuantity = dto.Quantity,
                ProducedQuantity = 0,
                Status = "Planned",
                PlannedStartDate = dto.StartDate,
                PlannedFinishDate = dto.FinishDate
            };

            _context.ProductionOrders.Add(order);

            // Reserve stock for each BOM item
            foreach (var item in bom.Items)
            {
                var requiredQty = item.Quantity * dto.Quantity;

                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ComponentId);

                if (stock == null)
                    return BadRequest($"Stock record not found for component {item.ComponentId}");

                if (stock.QuantityAvailable < requiredQty)
                    return BadRequest($"Not enough stock for component {item.ComponentId}");

                stock.QuantityAvailable -= requiredQty;
                stock.QuantityReserved += requiredQty;
                stock.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                order.Id,
                order.OrderNumber,
                order.ProductId,
                order.BillOfMaterialId,
                order.PlannedQuantity,
                order.Status,
                order.PlannedStartDate,
                order.PlannedFinishDate
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Error creating production order: {ex.Message}");
        }
    }

    [HttpPost("issue-material")]
    public async Task<IActionResult> IssueMaterialsForOrder(Guid orderId)
    {
        // Load order with BOM
        var order = await _context.ProductionOrders
            .Include(o => o.BillOfMaterials)
                .ThenInclude(b => b.Items) // BOM items (components)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound("Order not found");

        var bom = order.BillOfMaterials;

        if (bom == null || bom.Items == null || !bom.Items.Any())
            return BadRequest("No BOM defined for this product.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in bom.Items)
            {
                var requiredQty = item.Quantity * order.PlannedQuantity;

                // Get stock for component
                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ComponentId);

                if (stock == null)
                    return BadRequest($"Stock record not found for material {item.ComponentId}");

                if (stock.QuantityReserved < requiredQty)
                    return BadRequest($"Not enough reserved stock for material {item.ComponentId}");

                // Move stock: Reserved → InProduction
                stock.QuantityReserved -= requiredQty;
                stock.QuantityInProduction += requiredQty;
                stock.LastUpdated = DateTime.UtcNow;

                // Record consumption
                var existingConsumption = await _context.MaterialConsumptions
                    .FirstOrDefaultAsync(m => m.OrderId == orderId && m.MaterialId == item.ComponentId);

                if (existingConsumption != null)
                {
                    existingConsumption.ConsumedQuantity += requiredQty;
                    existingConsumption.PlannedQuantity = item.Quantity;
                    existingConsumption.ConsumptionDate = DateTime.UtcNow;
                }
                else
                {
                    _context.MaterialConsumptions.Add(new MaterialConsumption
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderId,
                        MaterialId = item.ComponentId,
                        PlannedQuantity = item.Quantity,
                        ConsumedQuantity = requiredQty,
                        ConsumptionDate = DateTime.UtcNow
                    });
                }

                // Record stock transaction
                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ComponentId,
                    Quantity = -requiredQty,
                    Type = "ISSUE",
                    ReferenceId = orderId,
                    Date = DateTime.UtcNow,
                    Notes = $"Issued for production order {order.OrderNumber}", // <-- Required
                    PerformedBy = "SYSTEM" // <-- Required field, replace with current user if available
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Materials issued successfully");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Error issuing materials: {ex.Message}");
        }
    }

    [HttpPost("start-production")]
    public async Task<IActionResult> StartProduction(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return BadRequest("Invalid OrderId.");

        var order = await _context.ProductionOrders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound("Production order not found.");

        if (order.Status == "Completed")
            return BadRequest("Production already completed.");

        if (order.Status == "InProgress")
            return BadRequest("Production already started.");

        if (order.Status != "Planned")
            return BadRequest("Only planned orders can be started.");

        order.Status = "InProgress";
        order.ActualStartDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Production started successfully.",
            Order = order
        });
    }

    [HttpPost("complete-production")]
    public async Task<IActionResult> CompleteProduction(CompleteProductionDto dto)
    {
        if (dto.OrderId == Guid.Empty)
            return BadRequest("Invalid OrderId.");

        if (dto.Quantity <= 0)
            return BadRequest("Produced quantity must be greater than zero.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.ProductionOrders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
                return NotFound("Production order not found.");

            if (order.Status != "InProgress")
                return BadRequest("Production must be started before completion.");

            // Update order
            order.ProducedQuantity = dto.Quantity;
            order.Status = "Completed";
            order.ActualFinishDate = DateTime.UtcNow;

            // Update finished product stock
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == order.ProductId);

            if (stock == null)
                return BadRequest("Product stock record not found.");

            stock.QuantityAvailable += dto.Quantity;
            stock.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            // Create receipt
            var receipt = new FinishedGoodsReceipt
            {
                OrderId = order.Id,
                ProductId = order.ProductId,
                Quantity = order.ProducedQuantity, // or whatever field you use
                ReceiptDate = DateTime.UtcNow
            };

            _context.FinishedGoodsReceipts.Add(receipt);

            await transaction.CommitAsync();

            return Ok(new
            {
                Message = "Production completed successfully.",
                Order = order
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
