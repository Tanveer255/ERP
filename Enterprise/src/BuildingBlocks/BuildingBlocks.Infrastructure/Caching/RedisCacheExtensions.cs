using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Caching;

public static class RedisCacheExtensions
{
    public static IServiceCollection AddRedisCaching(this IServiceCollection services, string connectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "erp:";
        });
        return services;
    }
}
