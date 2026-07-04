namespace Pana.Api.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Handlers implement IDomainEventHandler<TEvent>.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct);
}
