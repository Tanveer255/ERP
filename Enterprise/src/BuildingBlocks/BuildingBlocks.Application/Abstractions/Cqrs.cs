using BuildingBlocks.Domain;
using MediatR;

namespace BuildingBlocks.Application;

public interface ICommand<out TResponse> : IRequest<TResponse>;
public interface ICommand : IRequest;

public interface IQuery<out TResponse> : IRequest<TResponse>;

public abstract class CommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public abstract Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);
}

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
    Guid TenantId { get; }
    string EventType { get; }
}

public abstract record IntegrationEvent(Guid TenantId) : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
