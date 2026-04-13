using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity;
using ERP.Entity.Document;
using ERP.Entity.Product;
using ERP.Enum;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Document;

public class PurchaseOrderService
{
    private readonly ManufacturingDbContext _context;
    private readonly SalesOrderService _salesOrderService;
    public PurchaseOrderService(ManufacturingDbContext manufacturingDbContext,SalesOrderService salesOrderService)
    {
       _context = manufacturingDbContext;
         _salesOrderService = salesOrderService;
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
    public async Task ReceivePurchaseOrder(Guid purchaseOrderId)
    {
        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .ThenInclude(i => i.SalesOrderItem)
            .FirstOrDefaultAsync(p => p.Id == purchaseOrderId);

        foreach (var item in po.Items)
        {
            // 1️⃣ Add stock
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

            stock.QuantityAvailable += item.QuantityReceived;

            // 2️⃣ 🔥 TRIGGER SALES ORDER UPDATE
            if (item.SalesOrderItem != null)
            {
                await _salesOrderService.UpdateSalesOrderStock(
                    item.SalesOrderItem.SalesOrderId
                );
            }
        }

        await _context.SaveChangesAsync();
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
