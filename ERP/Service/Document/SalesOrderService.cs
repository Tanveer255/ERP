using ERP.Data;
using ERP.Entity.Product;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Document;

public class SalesOrderService
{
    private readonly ManufacturingDbContext _context;

    public SalesOrderService(ManufacturingDbContext context)
    {
        _context = context;
    }

    public async Task AutoReserveStock(Guid productId)
    {
        // Get all pending SO items for this product
        var items = await _context.SalesOrderItems
            .Include(i => i.SalesOrder)
            .Where(i =>
                i.ProductId == productId &&
                i.QuantityRequested > i.QuantityReserved)
            .OrderBy(i => i.SalesOrder.OrderDate) // FIFO
            .ToListAsync();

        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if (stock == null || stock.QuantityAvailable <= 0)
            return;

        foreach (var item in items)
        {
            var remaining = item.QuantityRequested - item.QuantityReserved;
            if (remaining <= 0) continue;

            var qty = Math.Min(remaining, stock.QuantityAvailable);

            stock.QuantityAvailable -= qty;
            stock.QuantityReserved += qty;

            item.QuantityReserved += qty;

            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = qty,
                Type = "AUTO-RESERVE",
                ReferenceId = item.SalesOrderId,
                Date = DateTime.UtcNow,
                PerformedBy = "SYSTEM",
                Notes = "Auto reserved after stock arrival"
            });

            if (stock.QuantityAvailable <= 0)
                break;
        }

        await _context.SaveChangesAsync();
    }
}