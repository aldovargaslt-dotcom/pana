namespace Pana.Api.Domain.Sales;

using Pana.Api.Domain.Common;

/// <summary>
/// A sale transaction with a state machine (Odoo-inspired).
/// States: Draft → Confirmed → Preparing → Ready → Completed / Voided
/// </summary>
public class Sale : TenantEntity
{
    public string Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }
    public Guid? SoldByUserId { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public static class Statuses
    {
        public const string Draft = "Draft";
        public const string Confirmed = "Confirmed";
        public const string Preparing = "Preparing";
        public const string Ready = "Ready";
        public const string Completed = "Completed";
        public const string Voided = "Voided";
    }

    /// <summary>
    /// Valid state transitions map.
    /// </summary>
    private static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        [Statuses.Draft] = new[] { Statuses.Confirmed, Statuses.Voided },
        [Statuses.Confirmed] = new[] { Statuses.Preparing, Statuses.Voided },
        [Statuses.Preparing] = new[] { Statuses.Ready, Statuses.Voided },
        [Statuses.Ready] = new[] { Statuses.Completed, Statuses.Voided },
    };

    private Sale() { } // EF Core

    public Sale(Guid tenantId, Guid? soldByUserId = null, string? notes = null)
        : base(tenantId)
    {
        Status = Statuses.Draft;
        SoldByUserId = soldByUserId;
        Notes = notes?.Trim();
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != Statuses.Draft)
            throw new InvalidOperationException("Cannot add items to a non-draft sale.");

        var item = new SaleItem(Id, productId, productName, unitPrice, quantity);
        _items.Add(item);
        RecalculateTotal();
    }

    public void Confirm()
    {
        TransitionTo(Statuses.Confirmed);
    }

    public void StartPreparing()
    {
        TransitionTo(Statuses.Preparing);
    }

    public void MarkReady()
    {
        TransitionTo(Statuses.Ready);
    }

    public void Complete()
    {
        TransitionTo(Statuses.Completed);
    }

    public void Void()
    {
        if (Status == Statuses.Voided || Status == Statuses.Completed)
            return;

        Status = Statuses.Voided;
        MarkUpdated();
    }

    private void TransitionTo(string targetStatus)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(targetStatus))
            throw new InvalidOperationException($"Cannot transition from '{Status}' to '{targetStatus}'. " +
                $"Allowed transitions: {string.Join(", ", allowed ?? Array.Empty<string>())}");

        Status = targetStatus;
        MarkUpdated();
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Where(i => !i.IsVoided).Sum(i => i.LineTotal);
    }
}
