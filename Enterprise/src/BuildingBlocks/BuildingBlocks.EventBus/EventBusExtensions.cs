using BuildingBlocks.Application;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.EventBus;

public static class EventBusExtensions
{
    public static IServiceCollection AddRabbitMqEventBus(
        this IServiceCollection services,
        string host,
        string username,
        string password,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            configureConsumers?.Invoke(x);
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        return services;
    }
}

public abstract class IntegrationEventConsumer<TEvent> : IConsumer<TEvent>
    where TEvent : class, IIntegrationEvent
{
    public abstract Task Consume(ConsumeContext<TEvent> context);
}
