using ERP.Data;
using ERP.Entity.Document;
using ERP.Entity;
using ERP.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity.DTO.Document;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SalesController : ControllerBase
{
    private readonly ManufacturingDbContext _context;

    public SalesController(ManufacturingDbContext context)
    {
        _context = context;
    }

    [HttpPost("create-sales-order")]
    public async Task<IActionResult> CreateSalesOrder(CreateSalesOrderDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            return BadRequest("Order must have items.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new SalesOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"SO-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                Status = SalesOrderStatus.Confirmed,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                Items = new List<SalesOrderItem>()
            };

            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return BadRequest($"Product {item.ProductId} not found");

                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                var unitPrice = product.UnitCost; // or selling price later
                var totalPrice = unitPrice * item.Quantity;

                totalAmount += totalPrice;

                //  STOCK CHECK
                if (stock == null || stock.QuantityAvailable < item.Quantity)
                {
                    var shortage = item.Quantity - (stock?.QuantityAvailable ?? 0);

                    //  Create Production Order automatically
                    var productionOrder = new ProductionOrder
                    {
                        Id = Guid.NewGuid(),
                        OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                        ProductId = item.ProductId,
                        PlannedQuantity = shortage,
                        ProducedQuantity = 0,
                        Status = nameof(ProductionStatus.Planned),
                        PlannedStartDate = DateTime.UtcNow,
                        PlannedFinishDate = DateTime.UtcNow.AddDays(2)
                    };

                    _context.ProductionOrders.Add(productionOrder);
                }
                else
                {
                    // Reserve stock
                    stock.QuantityAvailable -= item.Quantity;
                    stock.QuantityReserved += item.Quantity;
                }

                order.Items.Add(new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            order.TotalAmount = totalAmount;

            _context.SalesOrders.Add(order);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}
