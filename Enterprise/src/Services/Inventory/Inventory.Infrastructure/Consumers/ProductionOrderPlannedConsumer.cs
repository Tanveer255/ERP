using BuildingBlocks.EventBus.Events;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Consumers;

public class ProductionOrderPlannedConsumer(InventoryDbContext db) : IConsumer<ProductionOrderPlannedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductionOrderPlannedIntegrationEvent> context)
    {
        var msg = context.Message;
        db.StockTransactions.Add(new StockTransaction
        {
            TenantId = msg.TenantId,
            ProductId = msg.ProductId,
            WarehouseId = Guid.Empty,
            TransactionType = "MRP_PLANNED",
            Quantity = msg.Quantity,
            ReferenceType = "ProductionOrder",
            ReferenceId = msg.ProductionOrderId
        });
        await db.SaveChangesAsync();
    }
}

public static class InventoryConsumerRegistration
{
    public static void RegisterConsumers(IBusRegistrationConfigurator configurator) =>
        configurator.AddConsumer<ProductionOrderPlannedConsumer>();
}
