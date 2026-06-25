using BuildingBlocks.Application;
using FluentValidation;
using Manufacturing.Application.DTOs;

namespace Manufacturing.Application.Bom.Commands;

public record CreateBomCommand(Guid TenantId, Guid ProductId, IReadOnlyList<BomLineDto> Lines) : ICommand<BomDto>;

public class CreateBomCommandValidator : AbstractValidator<CreateBomCommand>
{
    public CreateBomCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty();
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ComponentProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
