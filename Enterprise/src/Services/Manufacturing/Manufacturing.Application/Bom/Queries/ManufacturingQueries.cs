using BuildingBlocks.Application;
using Manufacturing.Application.DTOs;

namespace Manufacturing.Application.Bom.Queries;

public record GetBomsQuery(Guid TenantId) : IQuery<IReadOnlyList<BomDto>>;
public record GetProductionOrdersQuery(Guid TenantId) : IQuery<IReadOnlyList<ProductionOrderDto>>;
