using BuildingBlocks.EventBus;
using Manufacturing.Domain.Repositories;
using Manufacturing.Infrastructure.Persistence;
using Manufacturing.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Manufacturing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddManufacturingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ManufacturingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ManufacturingDb")));

        services.AddScoped<IBomRepository, BomRepository>();
        services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
        services.AddScoped<BuildingBlocks.Domain.Repositories.IUnitOfWork, ManufacturingUnitOfWork>();

        services.AddRabbitMqEventBus(
            configuration["RabbitMq:Host"] ?? "localhost",
            configuration["RabbitMq:Username"] ?? "erp",
            configuration["RabbitMq:Password"] ?? "erp_secret");

        return services;
    }
}
