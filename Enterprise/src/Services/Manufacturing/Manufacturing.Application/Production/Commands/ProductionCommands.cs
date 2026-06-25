using BuildingBlocks.Application;
using FluentValidation;
using Manufacturing.Application.DTOs;

namespace Manufacturing.Application.Production.Commands;

public record CreateProductionOrderCommand(
    Guid TenantId,
    Guid ProductId,
    Guid BomId,
    decimal PlannedQuantity,
    DateTime PlannedStartDate,
    DateTime PlannedFinishDate) : ICommand<ProductionOrderDto>;

public class CreateProductionOrderCommandValidator : AbstractValidator<CreateProductionOrderCommand>
{
    public CreateProductionOrderCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.BomId).NotEmpty();
        RuleFor(x => x.PlannedQuantity).GreaterThan(0);
    }
}

public record ReleaseProductionOrderCommand(Guid TenantId, Guid ProductionOrderId) : ICommand<ProductionOrderDto>;
