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

    public MrpService(ManufacturingDbContext context)
    {
        _context = context;
    }

    public async Task RunMrpForSalesOrder(Guid salesOrderId)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == salesOrderId);

        if (order == null)
            throw new Exception("Sales Order not found");

        // Store POs grouped by supplier
        var purchaseOrdersMap = new Dictionary<Guid, PurchaseOrder>();

        foreach (var item in order.Items)
        {
            var stock = await _context.ProductStocks
                .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

            var availableStock = stock?.QuantityAvailable ?? 0;

            var incomingPOQty = await _context.PurchaseOrderItems
                .Include(p => p.PurchaseOrder)
                .Where(p => p.ProductId == item.ProductId &&
                            p.PurchaseOrder.Status != PurchaseOrderStatus.Received)
                .SumAsync(p => p.QuantityRequested - p.QuantityReceived);

            // 🔹 Reserve available stock first
            var remainingQty = item.QuantityRequested - item.QuantityReserved;
            if (remainingQty <= 0)
                continue;

            if (availableStock > 0)
            {
                var reserveQty = Math.Min(remainingQty, availableStock);

                stock.QuantityAvailable -= reserveQty;
                stock.QuantityReserved += reserveQty;

                item.QuantityReserved += reserveQty;

                // Optional: log reservation
                _context.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = reserveQty,
                    Type = "RESERVE",
                    ReferenceId = order.Id,
                    Date = DateTime.UtcNow,
                    PerformedBy = "System",
                    Notes = $"Reserved for Sales Order {order.OrderNumber}"
                });

                remainingQty -= reserveQty;
            }

            // 🔹 Check shortage after reservation and incoming POs
            var shortage = remainingQty - incomingPOQty;
            if (shortage <= 0)
                continue;

            // 🔹 Skip if already planned
            var alreadyPlanned = await _context.PurchaseOrderItems
                .AnyAsync(p => p.ProductId == item.ProductId && p.SalesOrderItemId == item.Id);
            var alreadyPlannedMO = await _context.ProductionOrders
                .AnyAsync(p => p.ProductId == item.ProductId && p.SalesOrderItemId == item.Id);

            if (alreadyPlanned || alreadyPlannedMO)
                continue;

            // 🔹 Check BOM for production
            var bom = await _context.BillOfMaterials
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.ProductId == item.ProductId);

            if (bom != null)
            {
                var productionOrder = new ProductionOrder
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = $"MO-{DateTime.UtcNow.Ticks}",
                    ProductId = item.ProductId,
                    BillOfMaterialId = bom.Id,
                    PlannedQuantity = shortage,
                    ProducedQuantity = 0,
                    Status = nameof(ProductionStatus.Planned),
                    PlannedStartDate = DateTime.UtcNow,
                    PlannedFinishDate = DateTime.UtcNow.AddDays(2),
                    SalesOrderItemId = item.Id
                };

                _context.ProductionOrders.Add(productionOrder);

                // Plan raw materials recursively
                foreach (var bomItem in bom.Items)
                {
                    var requiredQty = bomItem.Quantity * shortage;
                    await PlanMaterialRequirement(bomItem.ComponentId, requiredQty, item.Id);
                }
            }
            else
            {
                // 🔹 No BOM → create PO
                var supplier = await GetPreferredSupplierAsync(item.ProductId);
                if (supplier == null)
                    throw new Exception($"No supplier found for product {item.ProductId}");

                // Reuse PO if exists
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
        }

        //  Update Sales Order status based on reserved & fulfilled
        Helper.UpdateSalesOrderStatus(order);

        await _context.SaveChangesAsync();
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
