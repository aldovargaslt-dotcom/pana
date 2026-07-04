namespace Pana.Api.Domain.Inventory;

using Pana.Api.Domain.Common;

/// <summary>
/// Records a stock movement between locations. Current stock per location is derived from the sum of all movements.
/// This ledger approach is simpler and fully auditable — no separate Stock table needed.
/// Inspired by Odoo's stock.move with source/destination locations.
/// </summary>
public class InventoryMovement : TenantEntity
{
    public Guid ProductId { get; private set; }
    public string MovementType { get; private set; }
    public decimal Quantity { get; private set; }
    public string? Reason { get; private set; }
    public Guid? ReferenceSaleId { get; private set; }
    public Guid? PerformedByUserId { get; private set; }

    // ── Location support (Odoo-inspired) ──────────────────────
    public Guid? SourceLocationId { get; private set; }
    public Guid? DestinationLocationId { get; private set; }

    // ── Waste tracking ───────────────────────────────────────
    public Guid? WasteCategoryId { get; private set; }
    public Guid? WasteSubcategoryId { get; private set; }

    public static class Types
    {
        public const string StockIn = "StockIn";
        public const string StockOut = "StockOut";
        public const string Adjustment = "Adjustment";
        public const string SaleDeduction = "SaleDeduction";
        public const string Transfer = "Transfer";
        public const string ProductionIn = "ProductionIn";
        public const string ProductionOut = "ProductionOut";
    }

    private InventoryMovement() { } // EF Core

    public InventoryMovement(
        Guid tenantId,
        Guid productId,
        string movementType,
        decimal quantity,
        string? reason = null,
        Guid? referenceSaleId = null,
        Guid? performedByUserId = null,
        Guid? sourceLocationId = null,
        Guid? destinationLocationId = null,
        Guid? wasteCategoryId = null,
        Guid? wasteSubcategoryId = null)
        : base(tenantId)
    {
        if (movementType == Types.StockOut || movementType == Types.SaleDeduction || movementType == Types.ProductionOut)
        {
            // Stock going out should be recorded as positive; we store as negative
            Quantity = -Math.Abs(quantity);
        }
        else
        {
            Quantity = Math.Abs(quantity);
        }

        ProductId = productId;
        MovementType = movementType;
        Reason = reason?.Trim();
        ReferenceSaleId = referenceSaleId;
        PerformedByUserId = performedByUserId;
        SourceLocationId = sourceLocationId;
        DestinationLocationId = destinationLocationId;
        WasteCategoryId = wasteCategoryId;
        WasteSubcategoryId = wasteSubcategoryId;
    }

    public void SetWasteCategory(Guid? categoryId, Guid? subcategoryId = null)
    {
        WasteCategoryId = categoryId;
        WasteSubcategoryId = subcategoryId;
        MarkUpdated();
    }
}
