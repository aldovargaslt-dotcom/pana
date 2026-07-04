namespace Pana.Api.Domain.Operations;

using Pana.Api.Domain.Common;

/// <summary>
/// A single product line in a daily production capture.
/// Tracks the three key bakery metrics: initial count (leftover from yesterday),
/// production (baked today), and returns/waste (unsold or damaged).
/// </summary>
public class DailyProductionLine : BaseEntity
{
    public Guid DailyProductionId { get; private set; }
    public Guid ProductId { get; private set; }
    /// <summary>Snapshot of product name at time of capture (denormalized for history).</summary>
    public string ProductName { get; private set; }

    /// <summary>Unsold stock carried over from the previous day.</summary>
    public decimal Inicial { get; private set; }
    /// <summary>Units produced (baked) today.</summary>
    public decimal Produccion { get; private set; }
    /// <summary>Units returned, wasted, or damaged today.</summary>
    public decimal Devolucion { get; private set; }

    // ── Computed ─────────────────────────────────────────────
    /// <summary>Total available for sale = inicial + producción.</summary>
    public decimal Disponible => Inicial + Produccion;

    private DailyProductionLine() { } // EF Core

    public DailyProductionLine(Guid dailyProductionId, Guid productId, string productName, decimal inicial, decimal produccion, decimal devolucion)
    {
        DailyProductionId = dailyProductionId;
        ProductId = productId;
        ProductName = productName;
        Inicial = inicial;
        Produccion = produccion;
        Devolucion = devolucion;
    }

    public void Update(decimal inicial, decimal produccion, decimal devolucion)
    {
        Inicial = inicial;
        Produccion = produccion;
        Devolucion = devolucion;
        MarkUpdated();
    }
}
