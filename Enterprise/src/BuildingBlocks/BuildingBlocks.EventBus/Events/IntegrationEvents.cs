using BuildingBlocks.Application;

namespace BuildingBlocks.EventBus.Events;

public record SalesOrderCreatedIntegrationEvent(
    Guid TenantId,
    Guid SalesOrderId,
    string OrderNumber,
    decimal TotalAmount) : IntegrationEvent(TenantId);

public record ProductionOrderPlannedIntegrationEvent(
    Guid TenantId,
    Guid ProductionOrderId,
    Guid ProductId,
    decimal Quantity) : IntegrationEvent(TenantId);

public record PurchaseOrderReceivedIntegrationEvent(
    Guid TenantId,
    Guid PurchaseOrderId,
    Guid WarehouseId) : IntegrationEvent(TenantId);

public record StockAdjustedIntegrationEvent(
    Guid TenantId,
    Guid ProductId,
    Guid WarehouseId,
    decimal QuantityDelta,
    string TransactionType) : IntegrationEvent(TenantId);

public record MrpRunCompletedIntegrationEvent(
    Guid TenantId,
    Guid MrpRunId,
    int PlannedProductionCount,
    int PlannedPurchaseCount) : IntegrationEvent(TenantId);
