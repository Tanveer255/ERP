using FluentValidation;
using Manufacturing.Application.Bom.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Manufacturing.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddManufacturingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateBomCommandValidator>();
        return services;
    }
}
