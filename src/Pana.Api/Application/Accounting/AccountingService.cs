namespace Pana.Api.Application.Accounting;

using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Accounting;
using Pana.Api.Domain.Common;
using Pana.Api.Infrastructure.Data;

public interface IAccountingService
{
    Task<List<JournalEntryDto>> GetAllAsync(CancellationToken ct = default);
    Task<JournalEntryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<JournalEntryDto> CreateAsync(CreateJournalEntryRequest request, CancellationToken ct = default);
    Task<bool> PostAsync(Guid id, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid id, CancellationToken ct = default);
    Task<List<AccountBalanceDto>> GetTrialBalanceAsync(CancellationToken ct = default);
}

public class AccountingService : IAccountingService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public AccountingService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<JournalEntryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.JournalEntries
            .Include(j => j.Lines)
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.CreatedAt)
            .Select(j => MapToDto(j))
            .ToListAsync(ct);
    }

    public async Task<JournalEntryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _db.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        return entry is null ? null : MapToDto(entry);
    }

    public async Task<JournalEntryDto> CreateAsync(CreateJournalEntryRequest request, CancellationToken ct = default)
    {
        var entry = new JournalEntry(
            _tenantContext.TenantId,
            request.Reference,
            request.JournalType,
            request.EntryDate,
            request.Notes,
            request.SourceSaleId);

        foreach (var line in request.Lines)
        {
            entry.AddLine(line.Account, line.Description, line.Debit, line.Credit);
        }

        _db.JournalEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
        return MapToDto(entry);
    }

    public async Task<bool> PostAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _db.JournalEntries
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        if (entry is null) return false;

        entry.Post();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await _db.JournalEntries.FindAsync([id], ct);
        if (entry is null) return false;

        entry.Cancel();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<AccountBalanceDto>> GetTrialBalanceAsync(CancellationToken ct = default)
    {
        var postedLines = await _db.JournalEntryLines
            .Where(l => l.JournalEntry.Status == JournalEntry.Statuses.Posted)
            .GroupBy(l => l.Account)
            .Select(g => new AccountBalanceDto(
                g.Key,
                g.Sum(l => l.Debit),
                g.Sum(l => l.Credit),
                g.Sum(l => l.Debit) - g.Sum(l => l.Credit)
            ))
            .OrderBy(b => b.Account)
            .ToListAsync(ct);

        return postedLines;
    }

    private static JournalEntryDto MapToDto(JournalEntry j) => new(
        j.Id,
        j.Reference,
        j.JournalType,
        j.Status,
        j.EntryDate,
        j.Notes,
        j.SourceSaleId,
        j.TotalDebit,
        j.TotalCredit,
        j.CreatedAt,
        j.Lines.Select(l => new JournalEntryLineDto(l.Id, l.Account, l.Description, l.Debit, l.Credit)).ToList()
    );
}
