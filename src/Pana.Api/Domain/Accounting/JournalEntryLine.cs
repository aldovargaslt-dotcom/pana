namespace Pana.Api.Domain.Accounting;

using Pana.Api.Domain.Common;

/// <summary>
/// A single line in a journal entry — records one side of a double-entry transaction.
/// </summary>
public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; private set; }
    public string Account { get; private set; }
    public string Description { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    // Navigation property
    public JournalEntry JournalEntry { get; private set; } = null!;

    private JournalEntryLine() { }

    public JournalEntryLine(Guid journalEntryId, string account, string description, decimal debit, decimal credit)
    {
        if (string.IsNullOrWhiteSpace(account))
            throw new ArgumentException("Account is required.", nameof(account));
        if (debit < 0) throw new ArgumentException("Debit cannot be negative.", nameof(debit));
        if (credit < 0) throw new ArgumentException("Credit cannot be negative.", nameof(credit));
        if (debit == 0 && credit == 0)
            throw new ArgumentException("Either debit or credit must be positive.", nameof(debit));
        if (debit > 0 && credit > 0)
            throw new ArgumentException("A line cannot have both debit and credit.", nameof(debit));

        JournalEntryId = journalEntryId;
        Account = account.Trim();
        Description = description?.Trim() ?? string.Empty;
        Debit = debit;
        Credit = credit;
    }
}
