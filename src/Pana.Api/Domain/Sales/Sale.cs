namespace Pana.Api.Domain.Sales;

using Pana.Api.Domain.Common;

/// <summary>
/// A sale transaction with a state machine.
/// Order statuses: Draft → Confirmed → InProduction → Ready → Delivered / Cancelled
/// Payment statuses are tracked separately (Unpaid, PartiallyPaid, Paid, Refunded).
/// </summary>
public class Sale : TenantEntity
{
    // ── Order fields ──────────────────────────────────────────
    public string Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }
    public Guid? SoldByUserId { get; private set; }

    // ── Pre-order / customer fields ───────────────────────────
    public string OrderType { get; private set; } = OrderTypes.Standard;
    public string? CustomerName { get; private set; }
    public string? CustomerPhone { get; private set; }
    public DateTime? ScheduledDate { get; private set; }
    public decimal DepositAmount { get; private set; }
    public string PaymentStatus { get; private set; } = PaymentStatuses.Unpaid;
    public string? PaymentMethod { get; private set; }
    public string? InternalNotes { get; private set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    // ── Computed helpers ──────────────────────────────────────
    public decimal BalanceDue => TotalAmount - DepositAmount;
    public bool IsPreOrder => OrderType == OrderTypes.PreOrder;

    public static class OrderTypes
    {
        public const string Standard = "Standard";
        public const string PreOrder = "PreOrder";
    }

    public static class Statuses
    {
        public const string Draft = "Draft";
        public const string Confirmed = "Confirmed";
        public const string InProduction = "InProduction";
        public const string Ready = "Ready";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
    }

    public static class PaymentStatuses
    {
        public const string Unpaid = "Unpaid";
        public const string PartiallyPaid = "PartiallyPaid";
        public const string Paid = "Paid";
        public const string Refunded = "Refunded";
    }

    /// <summary>
    /// Valid order state transitions.
    /// </summary>
    private static readonly Dictionary<string, string[]> AllowedTransitions = new()
    {
        [Statuses.Draft] = new[] { Statuses.Confirmed, Statuses.Cancelled },
        [Statuses.Confirmed] = new[] { Statuses.InProduction, Statuses.Cancelled },
        [Statuses.InProduction] = new[] { Statuses.Ready, Statuses.Cancelled },
        [Statuses.Ready] = new[] { Statuses.Delivered, Statuses.Cancelled },
    };

    private Sale() { } // EF Core

    public Sale(Guid tenantId, Guid? soldByUserId = null, string? notes = null,
        string orderType = OrderTypes.Standard, string? customerName = null, string? customerPhone = null,
        DateTime? scheduledDate = null, decimal depositAmount = 0, string? paymentMethod = null,
        string? internalNotes = null)
        : base(tenantId)
    {
        Status = Statuses.Draft;
        SoldByUserId = soldByUserId;
        Notes = notes?.Trim();
        OrderType = orderType;
        CustomerName = customerName?.Trim();
        CustomerPhone = customerPhone?.Trim();
        ScheduledDate = scheduledDate;
        DepositAmount = depositAmount;
        PaymentMethod = paymentMethod?.Trim();
        InternalNotes = internalNotes?.Trim();

        if (orderType == OrderTypes.PreOrder && scheduledDate is null)
            throw new ArgumentException("ScheduledDate is required for pre-orders.");

        SetInitialPaymentStatus();
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != Statuses.Draft)
            throw new InvalidOperationException("Cannot add items to a non-draft sale.");

        var item = new SaleItem(Id, productId, productName, unitPrice, quantity);
        _items.Add(item);
        RecalculateTotal();
    }

    // ── Order state transitions ───────────────────────────────

    public void Confirm()
    {
        TransitionTo(Statuses.Confirmed);
    }

    public void StartProduction()
    {
        TransitionTo(Statuses.InProduction);
    }

    public void MarkReady()
    {
        TransitionTo(Statuses.Ready);
    }

    public void Deliver()
    {
        TransitionTo(Statuses.Delivered);
    }

    public void Cancel()
    {
        if (Status == Statuses.Cancelled || Status == Statuses.Delivered)
            return;

        Status = Statuses.Cancelled;
        MarkUpdated();
    }

    // ── Payment state transitions ─────────────────────────────

    public void RecordPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive.");

        DepositAmount += amount;
        SetInitialPaymentStatus();
        MarkUpdated();
    }

    public void MarkPaymentStatus(string targetStatus)
    {
        var valid = new[] { PaymentStatuses.Unpaid, PaymentStatuses.PartiallyPaid, PaymentStatuses.Paid, PaymentStatuses.Refunded };
        if (!valid.Contains(targetStatus))
            throw new ArgumentException($"Invalid payment status: {targetStatus}.");

        PaymentStatus = targetStatus;
        MarkUpdated();
    }

    // ── Pre-order detail setters (Draft only) ─────────────────

    public void SetPreOrderDetails(string customerName, string? customerPhone, DateTime scheduledDate,
        decimal depositAmount, string? internalNotes)
    {
        if (Status != Statuses.Draft)
            throw new InvalidOperationException("Cannot update pre-order details on a non-draft sale.");

        CustomerName = customerName?.Trim();
        CustomerPhone = customerPhone?.Trim();
        ScheduledDate = scheduledDate;
        DepositAmount = depositAmount;
        InternalNotes = internalNotes?.Trim();
        SetInitialPaymentStatus();
    }

    // ── Private helpers ───────────────────────────────────────

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

    private void SetInitialPaymentStatus()
    {
        if (DepositAmount <= 0)
            PaymentStatus = PaymentStatuses.Unpaid;
        else if (DepositAmount >= TotalAmount && TotalAmount > 0)
            PaymentStatus = PaymentStatuses.Paid;
        else
            PaymentStatus = PaymentStatuses.PartiallyPaid;
    }
}
