using ERP.Data;
using ERP.Entity;
using ERP.Entity.Document;
using ERP.Entity.DTO;
using ERP.Entity.Product;
using ERP.Enum;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Document;

public class PurchaseOrderService
{
    private readonly ManufacturingDbContext _context;
    public PurchaseOrderService(ManufacturingDbContext manufacturingDbContext)
    {
       _context = manufacturingDbContext;
    }

    public async Task<PurchaseOrder> CreatePurchaseOrder(Guid supplierId, List<PurchaseOrderItem> items)
    {
        var po = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"PO-{Guid.NewGuid():N8}",
            SupplierId = supplierId,
            OrderDate = DateTime.UtcNow,
            Status = PurchaseOrderStatus.Draft,
            Items = items
        };
        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync();
        return po;
    }
    public async Task<ResultDTO<PurchaseOrder>> GetPurchaseOrderByIdAsync(Guid purchaseOrderId)
    {
        var result =  await _context.PurchaseOrders
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == purchaseOrderId);
        if (result == null) 
            return ResultDTO<PurchaseOrder>.Failure("Purchase order not found");
        return ResultDTO<PurchaseOrder>.Success(result);
    }
    public async Task ReserveStockAndCreatePOs(ProductionOrder order, BillOfMaterial bom, Dictionary<Guid, PurchaseOrder> supplierOrders)
    {
        foreach (var item in bom.Items)
        {
            var requiredQty = item.Quantity * order.PlannedQuantity;
            var stock = await GetStock(item.ComponentId);

            var reserved = Math.Min(stock.QuantityAvailable, requiredQty);
            await AdjustStock(order.Id, item.ComponentId, reserved, StockTransactionType.RESERVE);

            var shortage = requiredQty - reserved;
            if (shortage > 0)
                await HandleSupplier(item, shortage, supplierOrders);
        }
    }
    private async Task<ProductStock> GetStock(Guid productId)
    {
        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if (stock == null)
            throw new Exception($"Stock not found for {productId}");

        return stock;
    }
    public async Task<bool> AdjustStock(Guid orderId, Guid productId, decimal qty, StockTransactionType type)
    {
        var stock = await GetStock(productId);

        switch (type)
        {
            case StockTransactionType.RESERVE:
                if (stock.QuantityAvailable < qty) return false;
                stock.QuantityAvailable -= qty;
                stock.QuantityReserved += qty;
                break;
            case StockTransactionType.ISSUE:
                if (stock.QuantityReserved < qty) return false;
                stock.QuantityReserved -= qty;
                stock.QuantityInProduction += qty;
                break;
            case StockTransactionType.CONSUME:
                stock.QuantityInProduction -= qty;
                break;
            case StockTransactionType.RECEIPT:
                stock.QuantityAvailable += qty;
                break;
        }

        AddStockTransaction(orderId, productId, qty, type);
        await _context.SaveChangesAsync();
        return true;
    }
    private async Task HandleSupplier(BillOfMaterialItem item, decimal shortage, Dictionary<Guid, PurchaseOrder> supplierOrders)
    {
        var supplier = await _context.ProductSuppliers
            .Where(x => x.ProductId == item.ComponentId)
            .OrderByDescending(x => x.IsPreferred)
            .ThenBy(x => x.Price)
            .FirstOrDefaultAsync();

        if (supplier == null)
            throw new Exception("No supplier found");

        var po = await GetOrCreateSupplierPO(supplier.SupplierId, supplierOrders);

        po.Items.Add(new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = item.ComponentId,
            QuantityRequested = shortage,
            UnitPrice = supplier.Price
        });
    }
    private void AddStockTransaction(Guid orderId, Guid productId, decimal qty, StockTransactionType type)
    {
        _context.StockTransactions.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = qty,
            Type = nameof(type),
            ReferenceId = orderId,
            Date = DateTime.UtcNow,
            PerformedBy = "SYSTEM"
        });
    }
    private async Task<PurchaseOrder> GetOrCreateSupplierPO(Guid supplierId, Dictionary<Guid, PurchaseOrder> supplierOrders)
    {
        if (!supplierOrders.ContainsKey(supplierId))
        {
            supplierOrders[supplierId] = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"PO-{Guid.NewGuid():N8}",
                SupplierId = supplierId,
                OrderDate = DateTime.UtcNow,
                Status = PurchaseOrderStatus.Draft,
                Items = new List<PurchaseOrderItem>()
            };
        }
        return supplierOrders[supplierId];
    }
}
