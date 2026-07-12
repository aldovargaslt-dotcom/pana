namespace Pana.Api.Domain.Operations;

using Pana.Api.Domain.Common;

/// <summary>
/// An individual record in the daily production timeline.
/// Each event captures a single action by a staff member:
/// setting the initial morning count, adding a production batch,
/// or registering a return/waste from the counter.
/// 
/// The daily totals per product are SUM(Quantity) grouped by EventType.
/// This replaces the old DailyProductionLine counter model.
/// </summary>
public class ProductionEvent : BaseEntity
{
    public Guid DailyProductionId { get; private set; }
    public Guid ProductId { get; private set; }
    /// <summary>Snapshot of product name at time of event.</summary>
    public string ProductName { get; private set; }
    public string EventType { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Notes { get; private set; }
    public Guid RegisteredByUserId { get; private set; }

    public static class Types
    {
        public const string Inicial = "Inicial";
        public const string Produccion = "Produccion";
        public const string Devolucion = "Devolucion";

        public static readonly string[] All = [Inicial, Produccion, Devolucion];
    }

    private ProductionEvent() { } // EF Core

    public ProductionEvent(
        Guid dailyProductionId,
        Guid productId,
        string productName,
        string eventType,
        decimal quantity,
        Guid registeredByUserId,
        string? notes = null)
    {
        if (!Types.All.Contains(eventType))
            throw new ArgumentException($"Invalid event type: {eventType}. Must be one of: {string.Join(", ", Types.All)}", nameof(eventType));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        DailyProductionId = dailyProductionId;
        ProductId = productId;
        ProductName = productName;
        EventType = eventType;
        Quantity = quantity;
        RegisteredByUserId = registeredByUserId;
        Notes = notes;
    }
}
