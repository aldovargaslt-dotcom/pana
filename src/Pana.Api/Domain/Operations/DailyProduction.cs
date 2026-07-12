namespace Pana.Api.Domain.Operations;

using Pana.Api.Domain.Common;

/// <summary>
/// Represents a daily production capture session for a specific day.
/// Tracks initial inventory, production output, and returns/waste per product.
/// Once closed, the counters are locked and InventoryMovements are generated.
/// This is the shop-floor operational core of the bakery workflow.
/// </summary>
public class DailyProduction : TenantEntity
{
    /// <summary>Links to the DailyContext for this day.</summary>
    public Guid DailyContextId { get; private set; }
    public DateTime Date { get; private set; }
    public bool IsClosed { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Guid? ClosedByUserId { get; private set; }

    private readonly List<DailyProductionLine> _lines = new();
    public IReadOnlyCollection<DailyProductionLine> Lines => _lines.AsReadOnly();

    private readonly List<ProductionEvent> _events = new();
    public IReadOnlyCollection<ProductionEvent> Events => _events.AsReadOnly();

    private DailyProduction() { } // EF Core

    public DailyProduction(Guid tenantId, Guid dailyContextId, DateTime date)
        : base(tenantId)
    {
        DailyContextId = dailyContextId;
        Date = date.Date;
    }

    public DailyProductionLine AddOrUpdateLine(Guid productId, string productName, decimal inicial, decimal produccion, decimal devolucion)
    {
        var existing = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (existing is not null)
        {
            existing.Update(inicial, produccion, devolucion);
            return existing;
        }

        var line = new DailyProductionLine(Id, productId, productName, inicial, produccion, devolucion);
        _lines.Add(line);
        MarkUpdated();
        return line;
    }

    public ProductionEvent AddEvent(Guid productId, string productName, string eventType, decimal quantity, Guid registeredByUserId, string? notes = null)
    {
        if (IsClosed)
            throw new InvalidOperationException("Cannot add events to a closed production day.");

        var evt = new ProductionEvent(Id, productId, productName, eventType, quantity, registeredByUserId, notes);
        _events.Add(evt);
        MarkUpdated();
        return evt;
    }

    public void Close(Guid userId)
    {
        if (IsClosed) return;

        IsClosed = true;
        ClosedAt = DateTime.UtcNow;
        ClosedByUserId = userId;
        MarkUpdated();
    }

    public void Reopen()
    {
        if (!IsClosed) return;

        IsClosed = false;
        ClosedAt = null;
        ClosedByUserId = null;
        MarkUpdated();
    }
}
