using ERP.Data;
using ERP.Entity;
using ERP.Entity.DTO;
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

        // Get BOM components
        var bomComponents = await _context.BillOfMaterials
            .Where(b => b.ProductId == dto.ProductId)
            .ToListAsync();

        if (!bomComponents.Any())
            return BadRequest("No BOM defined for this product.");
        // IMPORTANT: get BOM header ID
        var bomId = bomComponents.First().Id;

        // Create production order
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"PO-{DateTime.UtcNow.Ticks}",
            ProductId = dto.ProductId,
            BillOfMaterialId = bomId,
            PlannedQuantity = dto.Quantity,
            ProducedQuantity = 0,
            Status = "Planned",
            PlannedStartDate = dto.StartDate,
            PlannedFinishDate = dto.FinishDate
        };

        _context.ProductionOrders.Add(order);

        // Reserve required materials
        foreach (var component in bomComponents)
        {
            var requiredQty = component.Quantity * dto.Quantity;

            var stock = await _context.productStocks
                .FirstOrDefaultAsync(s => s.ProductId == component.ComponentId);

            if (stock == null || stock.QuantityAvailable < requiredQty)
                return BadRequest($"Not enough stock for component {component.ComponentId}");

            stock.QuantityAvailable -= requiredQty;
            stock.QuantityReserved += requiredQty;
            stock.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(order);
    }
    [HttpPost("issue-material")]
    public async Task<IActionResult> IssueMaterial(IssueMaterialDto dto)
    {
        if (dto.OrderId == Guid.Empty || dto.MaterialId == Guid.Empty)
            return BadRequest("Invalid OrderId or MaterialId.");

        if (dto.ConsumedQty <= 0)
            return BadRequest("Consumed quantity must be greater than zero.");

        // Check production order
        var order = await _context.ProductionOrders.FindAsync(dto.OrderId);
        if (order == null)
            return NotFound("Production order not found.");

        // Check material exists
        var product = await _context.Products.FindAsync(dto.MaterialId);
        if (product == null)
            return NotFound("Material not found.");

        // Get stock
        var stock = await _context.productStocks
            .FirstOrDefaultAsync(s => s.ProductId == dto.MaterialId);

        if (stock == null)
            return BadRequest("Stock record not found.");

        if (stock.QuantityReserved < dto.ConsumedQty)
            return BadRequest("Not enough reserved stock.");

        // Move stock Reserved → InProduction
        stock.QuantityReserved -= dto.ConsumedQty;
        stock.QuantityInProduction += dto.ConsumedQty;
        stock.LastUpdated = DateTime.UtcNow;

        // Record consumption
        var material = new MaterialConsumption
        {
            Id = Guid.NewGuid(),
            OrderId = dto.OrderId,
            MaterialId = dto.MaterialId,
            PlannedQuantity = dto.PlannedQty,
            ConsumedQuantity = dto.ConsumedQty,
            ConsumptionDate = DateTime.UtcNow
        };

        _context.MaterialConsumptions.Add(material);

        await _context.SaveChangesAsync();

        return Ok(material);
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
            var stock = await _context.productStocks
                .FirstOrDefaultAsync(s => s.ProductId == order.ProductId);

            if (stock == null)
                return BadRequest("Product stock record not found.");

            stock.QuantityAvailable += dto.Quantity;
            stock.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

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
