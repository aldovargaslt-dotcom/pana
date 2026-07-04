namespace Pana.Api.Domain.Sales;

using Pana.Api.Domain.Common;

/// <summary>
/// Raised when a sale is completed. Inventory listens to this to deduct stock.
/// </summary>
public record SaleItemSnapshot(Guid ProductId, decimal Quantity);

public record SaleCompletedEvent(Guid SaleId, Guid TenantId, List<SaleItemSnapshot> Items)
    : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
