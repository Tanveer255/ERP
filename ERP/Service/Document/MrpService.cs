using ERP.Data;
using ERP.Entity;
using ERP.Entity.Contact;
using ERP.Entity.Document;
using ERP.Entity.Product;
using ERP.Enum;
using Microsoft.EntityFrameworkCore;
using System;

namespace ERP.Service.Document;

public class MrpService 
{
    private readonly ManufacturingDbContext _context;
    private readonly SalesOrderService _salesOrderService;

    public MrpService(ManufacturingDbContext context, SalesOrderService salesOrderService)
    {
        _context = context;
        _salesOrderService = salesOrderService;
    }

    public async Task RunMrpForSalesOrder(Guid salesOrderId)
    {
        var salesOrderResponse = await _salesOrderService.LoadSalesOrderWithItems(salesOrderId);

        if (!salesOrderResponse.IsSuccess)
            throw new Exception(salesOrderResponse.Message);

        var salesOrder = salesOrderResponse.Data;

        if (salesOrder == null)
            throw new Exception("Sales order not found.");

        var purchaseOrdersByProduct = new Dictionary<Guid, PurchaseOrder>();

        foreach (var orderItem in salesOrder.Items)
        {
            // Attempt to reserve and fulfill available stock
            var unfulfilledQuantity = await ReserveAndFulfillStock(orderItem);

            // If stock is insufficient, plan procurement
            if (unfulfilledQuantity > 0)
            {
                await PlanShortage(orderItem, unfulfilledQuantity, purchaseOrdersByProduct);
            }
        }

        // Update overall sales order status based on fulfillment
        Helper.UpdateSalesOrderStatus(salesOrder);

        await _context.SaveChangesAsync();
    }

    private async Task<SalesOrder?> LoadSalesOrderWithItems(Guid salesOrderId)
    {
        return await _context.SalesOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == salesOrderId);
    }

    private async Task<decimal> ReserveAndFulfillStock(SalesOrderItem item)
    {
        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

        var availableStock = stock?.QuantityAvailable ?? 0;
        var remainingQty = item.QuantityRequested - item.QuantityFulfilled - item.QuantityReserved;

        if (remainingQty <= 0 || availableStock <= 0)
            return remainingQty;

        var reserveQty = Math.Min(remainingQty, availableStock);

        stock.QuantityAvailable -= reserveQty;
        stock.QuantityReserved += reserveQty;
        item.QuantityReserved += reserveQty;

        AddStockTransaction(item.ProductId, reserveQty, "RESERVE", item.SalesOrderId, $"Reserved for Sales Order {item.SalesOrderId}");

        // Auto fulfill reserved stock
        var fulfillQty = Math.Min(item.QuantityReserved - item.QuantityFulfilled, reserveQty);
        if (fulfillQty > 0)
        {
            item.QuantityFulfilled += fulfillQty;
            item.QuantityReserved -= fulfillQty;
            stock.QuantityReserved -= fulfillQty;

            AddStockTransaction(item.ProductId, fulfillQty, "FULFILL", item.SalesOrderId, $"Fulfilled for SO {item.SalesOrderId}");
        }

        return remainingQty - reserveQty;
    }
    private void AddStockTransaction(Guid productId, decimal qty, string type, Guid referenceId, string notes)
    {
        _context.StockTransactions.Add(new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = qty,
            Type = type,
            ReferenceId = referenceId,
            Date = DateTime.UtcNow,
            PerformedBy = "System",
            Notes = notes
        });
    }
    private async Task PlanShortage(SalesOrderItem item, decimal shortage, Dictionary<Guid, PurchaseOrder> purchaseOrdersMap)
    {
        // Check if already planned
        var alreadyPlanned = await _context.PurchaseOrderItems
            .AnyAsync(p => p.ProductId == item.ProductId && p.SalesOrderItemId == item.Id);
        var alreadyPlannedMO = await _context.ProductionOrders
            .AnyAsync(p => p.ProductId == item.ProductId && p.SalesOrderItemId == item.Id);

        if (alreadyPlanned || alreadyPlannedMO) return;

        // BOM exists → plan production
        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.ProductId == item.ProductId);

        if (bom != null)
        {
            await CreateProductionOrder(item, shortage, bom);
        }
        else
        {
            await CreatePurchaseOrder(item, shortage, purchaseOrdersMap);
        }
    }
    private async Task CreateProductionOrder(SalesOrderItem item, decimal quantity, BillOfMaterial bom)
    {
        var productionOrder = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"MO-{DateTime.UtcNow.Ticks}",
            ProductId = item.ProductId,
            BillOfMaterialId = bom.Id,
            PlannedQuantity = quantity,
            ProducedQuantity = 0,
            Status = nameof(ProductionStatus.Planned),
            PlannedStartDate = DateTime.UtcNow,
            PlannedFinishDate = DateTime.UtcNow.AddDays(2),
            SalesOrderItemId = item.Id
        };

        _context.ProductionOrders.Add(productionOrder);

        foreach (var bomItem in bom.Items)
        {
            var requiredQty = bomItem.Quantity * quantity;
            await PlanMaterialRequirement(bomItem.ComponentId, requiredQty, item.Id);
        }
    }
    private async Task CreatePurchaseOrder(SalesOrderItem item, decimal shortage, Dictionary<Guid, PurchaseOrder> purchaseOrdersMap)
    {
        var supplier = await GetPreferredSupplierAsync(item.ProductId);
        if (supplier == null) throw new Exception($"No supplier found for product {item.ProductId}");

        if (!purchaseOrdersMap.TryGetValue(supplier.SupplierId, out var po))
        {
            po = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                SupplierId = supplier.SupplierId,
                Status = PurchaseOrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                Items = new List<PurchaseOrderItem>()
            };

            purchaseOrdersMap[supplier.SupplierId] = po;
            _context.PurchaseOrders.Add(po);
        }

        po.Items.Add(new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = item.ProductId,
            QuantityRequested = shortage,
            QuantityReceived = 0,
            SalesOrderItemId = item.Id
        });
    }

    //  Recursive Material Planning
    private async Task PlanMaterialRequirement(Guid productId, decimal requiredQty, Guid salesOrderItemId)
    {
        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        var available = stock?.QuantityAvailable ?? 0;

        var shortage = requiredQty - available;
        if (shortage <= 0)
            return;

        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.ProductId == productId);

        if (bom != null)
        {
            // Nested production
            var mo = new ProductionOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"MO-{DateTime.UtcNow.Ticks}",
                ProductId = productId,
                BillOfMaterialId = bom.Id,
                PlannedQuantity = shortage,
                Status = nameof(ProductionStatus.Planned),
                SalesOrderItemId = salesOrderItemId
            };

            _context.ProductionOrders.Add(mo);

            foreach (var item in bom.Items)
            {
                await PlanMaterialRequirement(
                    item.ComponentId,
                    item.Quantity * shortage,
                    salesOrderItemId);
            }
        }
        else
        {
            // Purchase raw material
            var supplier = await GetPreferredSupplierAsync(productId);
            if (supplier == null)
                return;

            var po = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"AUTO-PO-{DateTime.UtcNow.Ticks}",
                SupplierId = supplier.SupplierId,
                Status = PurchaseOrderStatus.Draft,
                OrderDate = DateTime.UtcNow,
                Items = new List<PurchaseOrderItem>()
            };

            po.Items.Add(new PurchaseOrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                QuantityRequested = shortage,
                QuantityReceived = 0,
                SalesOrderItemId = salesOrderItemId
            });

            _context.PurchaseOrders.Add(po);
        }
    }

    private async Task<ProductSupplier?> GetPreferredSupplierAsync(Guid productId)
    {
        return await _context.ProductSuppliers
            .Where(s => s.ProductId == productId)
            .Select(s => new ProductSupplier
            {
                SupplierId = s.SupplierId,
                Price = s.Price
            })
            .FirstOrDefaultAsync();
    }
}
