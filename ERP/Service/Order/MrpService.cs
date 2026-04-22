using ERP.Data;
using ERP.Entity.BOM;
using ERP.Entity.Contact;
using ERP.Entity.Document;
using ERP.Entity.Order;
using ERP.Entity.Product;
using ERP.Enum;
using ERP.Service.Production;
using Microsoft.EntityFrameworkCore;
using System;

namespace ERP.Service.Document;

public class MrpService
{
    private readonly ManufacturingDbContext _context;
    private readonly SalesOrderService _salesOrderService;
    private readonly ProductionOrderService _productionOrderService;
    private readonly PurchaseOrderService _purchaseOrderService;

    public MrpService(ManufacturingDbContext context, SalesOrderService salesOrderService,ProductionOrderService productionOrderService)
    {
        _context = context;
        _salesOrderService = salesOrderService;
        _productionOrderService = productionOrderService;
    }

    public async Task RunMrpForSalesOrder(Guid salesOrderId)
    {
        var salesOrderResponse = await _salesOrderService.LoadSalesOrderWithItems(salesOrderId);
        if (!salesOrderResponse.IsSuccess) throw new Exception(salesOrderResponse.Message);

        var salesOrder = salesOrderResponse.Data ?? throw new Exception("Sales order not found.");

        var purchaseOrdersBySupplier = new Dictionary<Guid, PurchaseOrder>();
        var processedProducts = new HashSet<Guid>(); // NEW: track products already planned

        foreach (var item in salesOrder.Items)
        {
            var remainingQty = await ReserveStockForItemAsync(item);

            if (remainingQty > 0)
            {
                await PlanShortageAsync(item, remainingQty, purchaseOrdersBySupplier, processedProducts);
            }
        }

        Helper.UpdateSalesOrderStatus(salesOrder);
        await _context.SaveChangesAsync();
    }
    public async Task RunMrpForProductionOrder(Guid productionOrderId)
    {
        var productionOrderResponse = await _productionOrderService.LoadProductionOrderWithItems(productionOrderId);
        if (!productionOrderResponse.IsSuccess) throw new Exception(productionOrderResponse.Message);

        var productionOrder = productionOrderResponse.Data ?? throw new Exception("production order not found.");

        var purchaseOrdersBySupplier = new Dictionary<Guid, PurchaseOrder>();
        var processedProducts = new HashSet<Guid>(); // NEW: track products already planned

        //foreach (var item in productionOrder.Items)
        //{
        //    var remainingQty = await ReserveStockForItemAsync(item);

        //    if (remainingQty > 0)
        //    {
        //        await PlanShortageAsync(item, remainingQty, purchaseOrdersBySupplier, processedProducts);
        //    }
        //}

        await _context.SaveChangesAsync();
    }

    private async Task<decimal> ReserveStockForItemAsync(SalesOrderItem item)
    {
        // Load product info
        var product = await _context.Products
            .Where(p => p.Id == item.ProductId)
            .Select(p => new
            {
                p.IsManufactured,
                HasBOM = _context.BillOfMaterials.Any(b => b.ProductId == p.Id),
                p.IsPurchasable
            })
            .FirstOrDefaultAsync();

        if (product == null) throw new Exception($"Product not found: {item.ProductId}");

        // Only reserve stock if product is purchasable and not a manufactured BOM parent
        if (product.IsManufactured && product.HasBOM) return item.QuantityRequested - item.QuantityFulfilled;

        var remainingQty = item.QuantityRequested - item.QuantityFulfilled - item.QuantityReserved;
        if (remainingQty <= 0) return 0;

        var stock = await _context.ProductStocks
            .Where(s => s.ProductId == item.ProductId && s.QuantityAvailable > 0)
            .OrderByDescending(s => s.QuantityAvailable)
            .FirstOrDefaultAsync();

        if (stock == null) return remainingQty;

        var reserveQty = Math.Min(stock.QuantityAvailable, remainingQty);
        stock.QuantityAvailable -= reserveQty;
        stock.QuantityReserved += reserveQty;
        item.QuantityReserved += reserveQty;

        _context.StockTransactions.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = item.ProductId,
            Quantity = reserveQty,
            Type = "RESERVE",
            ReferenceId = item.SalesOrderId,
            Date = DateTime.UtcNow,
            PerformedBy = "System",
            Notes = $"Reserved for Sales Order {item.SalesOrderId}"
        });

        // Auto-fulfill
        var fulfillQty = Math.Min(item.QuantityReserved - item.QuantityFulfilled, reserveQty);
        if (fulfillQty > 0)
        {
            item.QuantityFulfilled += fulfillQty;
            item.QuantityReserved -= fulfillQty;
            stock.QuantityReserved -= fulfillQty;

            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = fulfillQty,
                Type = "FULFILL",
                ReferenceId = item.SalesOrderId,
                Date = DateTime.UtcNow,
                PerformedBy = "System",
                Notes = $"Fulfilled for Sales Order {item.SalesOrderId}"
            });
        }

        await _context.SaveChangesAsync();

        return remainingQty - reserveQty;
    }

    private async Task PlanShortageAsync(SalesOrderItem item, decimal shortageQty,
    Dictionary<Guid, PurchaseOrder> purchaseOrdersBySupplier,
    HashSet<Guid> processedProducts)
    {
        var product = await _context.Products
            .Where(p => p.Id == item.ProductId)
            .Select(p => new { p.IsManufactured })
            .FirstOrDefaultAsync();

        var bom = await _context.BillOfMaterials.Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.ProductId == item.ProductId);

        if (product.IsManufactured && bom != null)
        {
            await CreateProductionOrder(item, shortageQty, bom, purchaseOrdersBySupplier, processedProducts);
        }
        else
        {
            await CreatePurchaseOrder(item, shortageQty, purchaseOrdersBySupplier);
        }
    }

    // Add this parameter to keep track of already planned products
    private async Task CreateProductionOrder(SalesOrderItem item, decimal quantity, BillOfMaterial bom,
        Dictionary<Guid, PurchaseOrder> purchaseOrdersBySupplier,
        HashSet<Guid> processedProducts)
    {
        if (processedProducts.Contains(item.ProductId)) return;
        processedProducts.Add(item.ProductId);

        var mo = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"MO-{DateTime.UtcNow.Ticks}",
            ProductId = item.ProductId,
            BillOfMaterialId = bom.Id,
            PlannedQuantity = quantity,
            ProducedQuantity = 0,
            Status = nameof(ProductionStatus.Planned),
            SalesOrderItemId = item.Id
        };
        _context.ProductionOrders.Add(mo);

        foreach (var bomItem in bom.Items)
        {
            await PlanMaterialRequirement(bomItem.ComponentId, bomItem.Quantity * quantity, item.Id,
                purchaseOrdersBySupplier, processedProducts);
        }
    }

    private async Task PlanMaterialRequirement(Guid productId, decimal requiredQty, Guid salesOrderItemId,
    Dictionary<Guid, PurchaseOrder> purchaseOrdersBySupplier,
    HashSet<Guid> processedProducts)
    {
        if (processedProducts.Contains(productId)) return;
        processedProducts.Add(productId);

        var stock = await _context.ProductStocks.FirstOrDefaultAsync(s => s.ProductId == productId);
        var available = stock?.QuantityAvailable ?? 0;
        var shortage = requiredQty - available;
        if (shortage <= 0) return;

        var bom = await _context.BillOfMaterials.Include(b => b.Items).FirstOrDefaultAsync(b => b.ProductId == productId);
        if (bom != null)
        {
            await CreateProductionOrder(new SalesOrderItem { ProductId = productId, Id = salesOrderItemId }, shortage, bom,
                purchaseOrdersBySupplier, processedProducts);
        }
        else
        {
            await CreatePurchaseOrder(new SalesOrderItem { ProductId = productId, Id = salesOrderItemId }, shortage,
                purchaseOrdersBySupplier);
        }
    }

    private async Task CreatePurchaseOrder(SalesOrderItem item, decimal shortageQty, Dictionary<Guid, PurchaseOrder> purchaseOrdersBySupplier)
    {
        var supplier = await _context.ProductSuppliers
            .Where(s => s.ProductId == item.ProductId)
            .Select(s => new ProductSupplier { SupplierId = s.SupplierId, Price = s.Price })
            .FirstOrDefaultAsync();

        if (supplier == null) return;

        if (!purchaseOrdersBySupplier.TryGetValue(supplier.SupplierId, out var po))
        {
            po = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                SupplierId = supplier.SupplierId,
                Status = PurchaseOrderStatus.Draft,
                OrderDate = DateTime.UtcNow,
                Items = new List<PurchaseOrderItem>()
            };
            purchaseOrdersBySupplier[supplier.SupplierId] = po;
            _context.PurchaseOrders.Add(po);
        }

        po.Items.Add(new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = item.ProductId,
            QuantityRequested = shortageQty,
            QuantityReceived = 0,
            SalesOrderItemId = item.Id
        });
    }
}
