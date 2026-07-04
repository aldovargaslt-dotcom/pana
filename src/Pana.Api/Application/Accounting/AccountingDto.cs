namespace Pana.Api.Application.Accounting;

// ── DTOs ────────────────────────────────────────────────────────

public record JournalEntryLineDto(
    Guid Id,
    string Account,
    string Description,
    decimal Debit,
    decimal Credit
);

public record JournalEntryDto(
    Guid Id,
    string Reference,
    string JournalType,
    string Status,
    DateTime EntryDate,
    string? Notes,
    Guid? SourceSaleId,
    decimal TotalDebit,
    decimal TotalCredit,
    DateTime CreatedAt,
    List<JournalEntryLineDto> Lines
);

public record CreateJournalEntryRequest(
    string Reference,
    string JournalType,
    DateTime EntryDate,
    string? Notes,
    Guid? SourceSaleId,
    List<CreateJournalLineRequest> Lines
);

public record CreateJournalLineRequest(
    string Account,
    string Description,
    decimal Debit,
    decimal Credit
);

public record PostJournalEntryRequest();

public record AccountBalanceDto(
    string Account,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal Balance
);
