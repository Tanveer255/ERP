namespace Manufacturing.Application.DTOs;

public record BomLineDto(Guid ComponentProductId, decimal Quantity, string Unit);
public record BomDto(Guid Id, Guid ProductId, string Version, bool IsActive, IReadOnlyList<BomLineDto> Lines);
public record ProductionOrderDto(
    Guid Id,
    string OrderNumber,
    Guid ProductId,
    Guid BomId,
    decimal PlannedQuantity,
    decimal ProducedQuantity,
    string Status,
    DateTime PlannedStartDate,
    DateTime PlannedFinishDate);
