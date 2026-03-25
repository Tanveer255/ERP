using ERP.Data;
using ERP.Entity.Document;
using ERP.Entity;
using ERP.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ERP.Entity.DTO.Document;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Product;

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
            // Create the sales order
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
                // Fetch product with its prices
                var product = await _context.Products
                    .Include(p => p.Prices)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                    return BadRequest($"Product {item.ProductId} not found");

                var stock = await _context.ProductStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                // Get unit price (use the first price or apply logic for active price)
                var unitPrice = product.Prices.FirstOrDefault()?.SalePrice ?? 0;
                var totalPrice = unitPrice * item.Quantity;

                totalAmount += totalPrice;

                // STOCK CHECK
                if (stock == null || stock.QuantityAvailable < item.Quantity)
                {
                    var shortage = item.Quantity - (stock?.QuantityAvailable ?? 0);

                    // Create Production Order automatically for shortage
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

                    // If some stock exists, reserve what is available
                    if (stock != null && stock.QuantityAvailable > 0)
                    {
                        var availableQty = stock.QuantityAvailable;
                        stock.QuantityAvailable -= availableQty;
                        stock.QuantityReserved += availableQty;

                        // Add stock transaction for reserved quantity
                        _context.StockTransactions.Add(new StockTransaction
                        {
                            Id = Guid.NewGuid(),
                            ProductId = item.ProductId,
                            Quantity = availableQty,
                            Type = "RESERVE",
                            ReferenceId = order.Id,
                            Date = DateTime.UtcNow,
                            PerformedBy = dto.CustomerName,
                            Notes = $"Reserved {availableQty} units for Sales Order {order.OrderNumber}"
                        });
                    }
                }
                else
                {
                    // Reserve full requested quantity
                    stock.QuantityAvailable -= item.Quantity;
                    stock.QuantityReserved += item.Quantity;

                    // Add stock transaction for reserved quantity
                    _context.StockTransactions.Add(new StockTransaction
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Type = "RESERVE",
                        ReferenceId = order.Id,
                        Date = DateTime.UtcNow,
                        PerformedBy = dto.CustomerName,
                        Notes = $"Reserved {item.Quantity} units for Sales Order {order.OrderNumber}"
                    });
                }

                // Add item to sales order
                order.Items.Add(new SalesOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice
                });
            }

            // Set total amount
            order.TotalAmount = totalAmount;

            // Save the sales order
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
