using BuildingBlocks.Application;
using BuildingBlocks.EventBus.Events;
using Manufacturing.Application.Bom.Commands;
using Manufacturing.Application.DTOs;
using Manufacturing.Application.Bom.Queries;
using Manufacturing.Application.Production.Commands;
using Manufacturing.Domain.Entities;
using Manufacturing.Domain.Repositories;
using MassTransit;
using MediatR;

namespace Manufacturing.Application.Bom.Handlers;

public class CreateBomCommandHandler(
    IBomRepository bomRepository,
    BuildingBlocks.Domain.Repositories.IUnitOfWork unitOfWork) : IRequestHandler<CreateBomCommand, BomDto>
{
    public async Task<BomDto> Handle(CreateBomCommand request, CancellationToken cancellationToken)
    {
        var bom = new BillOfMaterial
        {
            TenantId = request.TenantId,
            ProductId = request.ProductId,
            Version = "1.0",
            IsActive = true,
            Lines = request.Lines.Select(l => new BomLine
            {
                ComponentProductId = l.ComponentProductId,
                Quantity = l.Quantity,
                Unit = l.Unit
            }).ToList()
        };

        await bomRepository.AddAsync(bom, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(bom);
    }

    internal static BomDto Map(BillOfMaterial bom) =>
        new(bom.Id, bom.ProductId, bom.Version, bom.IsActive,
            bom.Lines.Select(l => new BomLineDto(l.ComponentProductId, l.Quantity, l.Unit)).ToList());
}

public class CreateProductionOrderCommandHandler(
    IProductionOrderRepository productionOrderRepository,
    BuildingBlocks.Domain.Repositories.IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint) : IRequestHandler<CreateProductionOrderCommand, ProductionOrderDto>
{
    public async Task<ProductionOrderDto> Handle(CreateProductionOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new ProductionOrder
        {
            TenantId = request.TenantId,
            OrderNumber = $"MO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ProductId = request.ProductId,
            BomId = request.BomId,
            PlannedQuantity = request.PlannedQuantity,
            PlannedStartDate = request.PlannedStartDate,
            PlannedFinishDate = request.PlannedFinishDate,
            Status = "Planned"
        };

        await productionOrderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new ProductionOrderPlannedIntegrationEvent(
            request.TenantId, order.Id, order.ProductId, order.PlannedQuantity), cancellationToken);

        return ProductionOrderHandlers.Map(order);
    }
}

public class ReleaseProductionOrderCommandHandler(
    IProductionOrderRepository productionOrderRepository,
    BuildingBlocks.Domain.Repositories.IUnitOfWork unitOfWork) : IRequestHandler<ReleaseProductionOrderCommand, ProductionOrderDto>
{
    public async Task<ProductionOrderDto> Handle(ReleaseProductionOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await productionOrderRepository.GetByIdAsync(request.ProductionOrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Production order not found.");

        if (order.TenantId != request.TenantId)
            throw new UnauthorizedAccessException();

        order.Status = "Released";
        productionOrderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductionOrderHandlers.Map(order);
    }
}

public static class ProductionOrderHandlers
{
    public static ProductionOrderDto Map(ProductionOrder order) =>
        new(order.Id, order.OrderNumber, order.ProductId, order.BomId, order.PlannedQuantity,
            order.ProducedQuantity, order.Status, order.PlannedStartDate, order.PlannedFinishDate);
}

public class GetBomsQueryHandler(IBomRepository bomRepository) : IRequestHandler<GetBomsQuery, IReadOnlyList<BomDto>>
{
    public async Task<IReadOnlyList<BomDto>> Handle(GetBomsQuery request, CancellationToken cancellationToken)
    {
        var boms = await bomRepository.GetByTenantAsync(request.TenantId, cancellationToken);
        return boms.Select(CreateBomCommandHandler.Map).ToList();
    }
}

public class GetProductionOrdersQueryHandler(IProductionOrderRepository productionOrderRepository)
    : IRequestHandler<GetProductionOrdersQuery, IReadOnlyList<ProductionOrderDto>>
{
    public async Task<IReadOnlyList<ProductionOrderDto>> Handle(GetProductionOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await productionOrderRepository.GetByTenantAsync(request.TenantId, cancellationToken);
        return orders.Select(ProductionOrderHandlers.Map).ToList();
    }
}
