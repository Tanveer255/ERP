using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity.Product;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Product;

public class ProductStockService
{
    private ManufacturingDbContext _context;
    public ProductStockService(ManufacturingDbContext context)
    {
        _context = context;
    }
    public async Task<ResultDTO<ProductStock>> GetStockStockByProductId(Guid productId, CancellationToken cancellationToken = default)
    {
        var result = await _context.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == productId, cancellationToken);
        if (result ==null)
        {
                return ResultDTO<ProductStock>.Failure($"Product {productId} not found.");
        }
        return ResultDTO<ProductStock>.Success(result);
    }
    public async Task<bool> CheckStockAvailability(Guid orderId)
    {
        // Load order with BOM
        var order = await _context.ProductionOrders
            .Include(o => o.BillOfMaterials)
                .ThenInclude(b => b.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new Exception("Order not found");

        foreach (var item in order.BillOfMaterials.Items)
        {
            var requiredQty = item.Quantity * order.PlannedQuantity;

            // Get stock record
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == item.ComponentId);

            var availableQty = stock?.QuantityAvailable ?? 0;

            if (availableQty < requiredQty)
            {
                return false; 
            }
        }

        return true;
    }
}
