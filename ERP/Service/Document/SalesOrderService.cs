using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity.Document;
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
    public async Task<ResultDTO<SalesOrder>> LoadSalesOrderWithItems(Guid salesOrderId)
    {
        var result  = await _context.SalesOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == salesOrderId);
        if (result == null)
            return ResultDTO<SalesOrder>.Failure("Sales order not found.");
        return ResultDTO<SalesOrder>.Success(result);
    }
    public async Task UpdateSalesOrderStock(Guid salesOrderId)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == salesOrderId);

        if (order == null) return;

        foreach (var item in order.Items)
        {
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

            if (stock == null || stock.QuantityAvailable <= 0)
                continue;

            var remainingQty = item.QuantityRequested - item.QuantityReserved;
            if (remainingQty <= 0)
                continue;

            var qtyToReserve = Math.Min(remainingQty, stock.QuantityAvailable);

            stock.QuantityAvailable -= qtyToReserve;
            stock.QuantityReserved += qtyToReserve;

            item.QuantityReserved += qtyToReserve;
        }

        Helper.UpdateSalesOrderStatus(order);

        await _context.SaveChangesAsync();
    }
}