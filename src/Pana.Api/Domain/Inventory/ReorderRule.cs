namespace Pana.Api.Domain.Inventory;

using Pana.Api.Domain.Common;

/// <summary>
/// Automatic reorder rule — triggers when stock falls below minimum.
/// Inspired by Odoo's stock.warehouse.orderpoint.
/// </summary>
public class ReorderRule : TenantEntity
{
    public Guid ProductId { get; private set; }
    public Guid LocationId { get; private set; }
    public decimal MinimumStock { get; private set; }
    public decimal MaximumStock { get; private set; }
    public decimal ReorderQuantity { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastTriggeredAt { get; private set; }

    private ReorderRule() { }

    public ReorderRule(Guid tenantId, Guid productId, Guid locationId, decimal minimumStock, decimal maximumStock, decimal reorderQuantity)
        : base(tenantId)
    {
        if (minimumStock < 0)
            throw new ArgumentException("Minimum stock cannot be negative.", nameof(minimumStock));
        if (maximumStock <= minimumStock)
            throw new ArgumentException("Maximum stock must be greater than minimum.", nameof(maximumStock));
        if (reorderQuantity <= 0)
            throw new ArgumentException("Reorder quantity must be positive.", nameof(reorderQuantity));

        ProductId = productId;
        LocationId = locationId;
        MinimumStock = minimumStock;
        MaximumStock = maximumStock;
        ReorderQuantity = reorderQuantity;
    }

    public void UpdateThresholds(decimal minimumStock, decimal maximumStock, decimal reorderQuantity)
    {
        if (minimumStock < 0)
            throw new ArgumentException("Minimum stock cannot be negative.", nameof(minimumStock));
        if (maximumStock <= minimumStock)
            throw new ArgumentException("Maximum stock must be greater than minimum.", nameof(maximumStock));
        if (reorderQuantity <= 0)
            throw new ArgumentException("Reorder quantity must be positive.", nameof(reorderQuantity));

        MinimumStock = minimumStock;
        MaximumStock = maximumStock;
        ReorderQuantity = reorderQuantity;
        MarkUpdated();
    }

    public void MarkTriggered()
    {
        LastTriggeredAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    /// <summary>
    /// Checks if current stock level triggers this reorder rule.
    /// </summary>
    public bool ShouldTrigger(decimal currentStock) => IsActive && currentStock <= MinimumStock;
}
