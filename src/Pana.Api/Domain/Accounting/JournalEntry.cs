namespace Pana.Api.Domain.Accounting;

using Pana.Api.Domain.Common;

/// <summary>
/// A journal entry representing a financial transaction (double-entry bookkeeping).
/// Inspired by Odoo's account.move — each entry has balanced debit/credit lines.
/// </summary>
public class JournalEntry : TenantEntity
{
    public string Reference { get; private set; }
    public string JournalType { get; private set; }
    public string Status { get; private set; }
    public DateTime EntryDate { get; private set; }
    public string? Notes { get; private set; }
    public Guid? SourceSaleId { get; private set; }

    private readonly List<JournalEntryLine> _lines = new();
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    public decimal TotalDebit => _lines.Sum(l => l.Debit);
    public decimal TotalCredit => _lines.Sum(l => l.Credit);
    public bool IsBalanced => TotalDebit == TotalCredit;

    public static class JournalTypes
    {
        public const string Sale = "Sale";
        public const string Purchase = "Purchase";
        public const string Cash = "Cash";
        public const string Bank = "Bank";
        public const string General = "General";
    }

    public static class Statuses
    {
        public const string Draft = "Draft";
        public const string Posted = "Posted";
        public const string Cancelled = "Cancelled";
    }

    private JournalEntry() { }

    public JournalEntry(Guid tenantId, string reference, string journalType, DateTime entryDate, string? notes = null, Guid? sourceSaleId = null)
        : base(tenantId)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Reference is required.", nameof(reference));

        Reference = reference.Trim();
        JournalType = journalType;
        Status = Statuses.Draft;
        EntryDate = entryDate;
        Notes = notes?.Trim();
        SourceSaleId = sourceSaleId;
    }

    public void AddLine(string account, string description, decimal debit, decimal credit)
    {
        if (Status != Statuses.Draft)
            throw new InvalidOperationException("Cannot add lines to a non-draft journal entry.");

        var line = new JournalEntryLine(Id, account, description, debit, credit);
        _lines.Add(line);
    }

    public void Post()
    {
        if (Status != Statuses.Draft)
            throw new InvalidOperationException("Only draft entries can be posted.");

        if (!IsBalanced)
            throw new InvalidOperationException($"Journal entry is not balanced. Debit: {TotalDebit}, Credit: {TotalCredit}");

        Status = Statuses.Posted;
        MarkUpdated();
    }

    public void Cancel()
    {
        if (Status == Statuses.Cancelled)
            return;
        Status = Statuses.Cancelled;
        MarkUpdated();
    }
}
