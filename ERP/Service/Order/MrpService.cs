using ERP.Data;
using ERP.Data.DTO.MRP;
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

    public MrpService(
        ManufacturingDbContext context,
        SalesOrderService salesOrderService,
        ProductionOrderService productionOrderService)
    {
        _context = context;
        _salesOrderService = salesOrderService;
        _productionOrderService = productionOrderService;
    }

    #region PUBLIC ENTRY POINTS

    public async Task RunMrpForSalesOrder(Guid salesOrderId)
    {
        var salesOrderResponse = await _salesOrderService.LoadSalesOrderWithItems(salesOrderId);
        if (!salesOrderResponse.IsSuccess)
            throw new Exception(salesOrderResponse.Message);

        var salesOrder = salesOrderResponse.Data
            ?? throw new Exception("Sales order not found.");

        var demands = salesOrder.Items.Select(x => new MrpContextDTO
        {
            ProductId = x.ProductId,
            QuantityRequested = x.QuantityRequested,
            ReferenceId = salesOrderId,
            SourceType = MrpSourceType.Sales
        }).ToList();

        await RunMrp(demands);
        Helper.UpdateSalesOrderStatus(salesOrder);

        await _context.SaveChangesAsync();
    }

    public async Task RunMrpForProductionShortage(Guid productionOrderId)
    {
        var orderResponse = await _productionOrderService.LoadProductionOrderWithItems(productionOrderId);
        if (!orderResponse.IsSuccess)
            throw new Exception(orderResponse.Message);

        var order = orderResponse.Data
            ?? throw new Exception("Production order not found.");

        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == order.BillOfMaterialId);

        if (bom == null) return;

        var demands = bom.Items.Select(x => new MrpContextDTO
        {
            ProductId = x.ComponentId,
            QuantityRequested = x.Quantity * order.PlannedQuantity,
            ReferenceId = productionOrderId,
            SourceType = MrpSourceType.Production
        }).ToList();

        await RunMrp(demands);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region CORE MRP ENGINE

    private async Task RunMrp(List<MrpContextDTO> demands)
    {
        var purchaseOrdersBySupplier = new Dictionary<Guid, PurchaseOrder>();
        var processedProducts = new HashSet<Guid>();

        foreach (var demand in demands)
        {
            await ProcessDemand(demand, purchaseOrdersBySupplier, processedProducts);
        }

        // bulk save handled by caller or here if needed
    }

    private async Task ProcessDemand(
        MrpContextDTO demand,
        Dictionary<Guid, PurchaseOrder> purchaseOrdersBySupplier,
        HashSet<Guid> processedProducts)
    {
        if (processedProducts.Contains(demand.ProductId))
            return;

        processedProducts.Add(demand.ProductId);

        var stock = await _context.ProductStocks
            .FirstOrDefaultAsync(x => x.ProductId == demand.ProductId);

        var available = stock?.QuantityAvailable ?? 0;

        if (available >= demand.QuantityRequested)
            return;

        var shortage = demand.QuantityRequested - available;

        var product = await _context.Products
            .Where(p => p.Id == demand.ProductId)
            .Select(p => new { p.IsManufactured })
            .FirstOrDefaultAsync();

        var bom = await _context.BillOfMaterials
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.ProductId == demand.ProductId);

        // =========================
        // CASE 1: MANUFACTURED ITEM
        // =========================
        if (product?.IsManufactured == true && bom != null)
        {
            foreach (var item in bom.Items)
            {
                var childDemand = new MrpContextDTO
                {
                    ProductId = item.ComponentId,
                    QuantityRequested = item.Quantity * shortage,
                    ReferenceId = demand.ReferenceId,
                    SourceType = demand.SourceType
                };

                await ProcessDemand(childDemand, purchaseOrdersBySupplier, processedProducts);
            }

            return;
        }

        // =========================
        // CASE 2: PURCHASE ITEM
        // =========================
        await CreatePurchaseOrder(
            demand.ProductId,
            shortage,
            demand.SourceType == MrpSourceType.Sales ? demand.ReferenceId : null,
            demand.SourceType == MrpSourceType.Production ? demand.ReferenceId : null,
            purchaseOrdersBySupplier
        );
    }

    #endregion

    #region PURCHASE ORDER CREATION

    private async Task CreatePurchaseOrder(
        Guid productId,
        decimal shortageQty,
        Guid? salesOrderId,
        Guid? productionOrderId,
        Dictionary<Guid, PurchaseOrder> purchaseOrdersBySupplier)
    {
        var supplier = await _context.ProductSuppliers
            .Where(s => s.ProductId == productId)
            .Select(s => new ProductSupplier { SupplierId = s.SupplierId })
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
            ProductId = productId,
            QuantityRequested = shortageQty,

            SalesOrderItemId = salesOrderId,
            ProductionOrderId = productionOrderId
        });
    }

    #endregion
}