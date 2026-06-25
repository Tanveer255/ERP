using FluentValidation;
using Identity.Application.Auth.Commands;
using Identity.Application.Auth.Handlers;
using Identity.Application.Auth.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();
        return services;
    }
}
