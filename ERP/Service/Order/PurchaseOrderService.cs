using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity.BOM;
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

            // 2️⃣  TRIGGER SALES ORDER UPDATE
            if (item.SalesOrderItem != null)
            {
                await _salesOrderService.UpdateSalesOrderStock(
                    item.SalesOrderItem.SalesOrderId
                );
            }
        }

        await _context.SaveChangesAsync();
    }
    public async Task<ProductStock> GetStock(Guid productId)
    {
        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if (stock == null)
            throw new Exception($"Stock not found for {productId}");

        return stock;
    }
    public async Task<bool> AdjustStock(
    Guid orderId,
    Guid productId,
    decimal qty,
    StockTransactionType type)
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
                // 1. Increase stock
                stock.QuantityAvailable += qty;

                // 2. Save transaction first (important for consistency)
                AddStockTransaction(orderId, productId, qty, type);
                await _context.SaveChangesAsync();

                // 3. AUTO RESERVATION TRIGGER (IMPORTANT PART)
                await AutoAllocateStock(productId);

                return true;
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
    public void AddStockTransaction(Guid orderId, Guid productId, decimal qty, StockTransactionType type)
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
    public async Task AutoAllocateStock(Guid productId)
    {
        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(x => x.ProductId == productId);

        if (stock == null || stock.QuantityAvailable <= 0)
            return;

        var available = stock.QuantityAvailable;

        var demands = await GetComponentDemand(productId);

        foreach (var d in demands)
        {
            if (available <= 0)
                break;

            var allocate = Math.Min(d.RequiredQty, available);

            if (allocate <= 0)
                continue;

            stock.QuantityAvailable -= allocate;
            stock.QuantityReserved += allocate;

            available -= allocate;
        }

        stock.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
    private async Task<List<(Guid ProductId, decimal RequiredQty)>> GetComponentDemand(Guid productId)
    {
        var orders = await _context.ProductionOrders
            .Where(o => o.Status == nameof(ProductionStatus.Planned)
                     || o.Status == nameof(ProductionStatus.Ready))
            .ToListAsync();

        var result = new List<(Guid, decimal)>();

        foreach (var order in orders)
        {
            var bom = await _context.BillOfMaterials
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.ProductId == order.ProductId);

            if (bom == null) continue;

            foreach (var item in bom.Items)
            {
                if (item.ComponentId != productId)
                    continue;

                var required = item.Quantity * order.PlannedQuantity;

                result.Add((item.ComponentId, required));
            }
        }

        return result;
    }
}
