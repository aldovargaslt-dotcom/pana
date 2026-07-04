using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Common;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Common;

public interface IDailyContextService
{
    Task<DailyContextDto?> GetTodayAsync(CancellationToken ct = default);
    Task<DailyContextDto?> GetByDateAsync(DateTime date, CancellationToken ct = default);
    Task<List<DailyContextDto>> GetRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<DailyContextDto> UpsertAsync(DailyContextUpsertRequest request, CancellationToken ct = default);
    Task CloseTodayAsync(CancellationToken ct = default);
}

public class DailyContextService : IDailyContextService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public DailyContextService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<DailyContextDto?> GetTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        return await GetByDateAsync(today, ct);
    }

    public async Task<DailyContextDto?> GetByDateAsync(DateTime date, CancellationToken ct = default)
    {
        var entity = await _db.DailyContexts
            .FirstOrDefaultAsync(d => d.Date == date.Date, ct);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<List<DailyContextDto>> GetRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _db.DailyContexts
            .Where(d => d.Date >= from.Date && d.Date <= to.Date)
            .OrderBy(d => d.Date)
            .Select(d => MapToDto(d))
            .ToListAsync(ct);
    }

    public async Task<DailyContextDto> UpsertAsync(DailyContextUpsertRequest request, CancellationToken ct = default)
    {
        var date = request.Date.Date;
        var existing = await _db.DailyContexts
            .FirstOrDefaultAsync(d => d.Date == date, ct);

        DailyContext entity;

        if (existing is null)
        {
            entity = new DailyContext(_tenantContext.TenantId, date, request.DayType ?? DailyContext.DayTypes.Normal);
            _db.DailyContexts.Add(entity);
        }
        else
        {
            entity = existing;
        }

        if (request.Weather is not null) entity.SetWeather(request.Weather);
        if (request.DayType is not null) entity.SetDayType(request.DayType);
        entity.SetIsPayday(request.IsPayday);
        entity.SetHasLocalEvent(request.HasLocalEvent);
        entity.SetSchoolOut(request.SchoolOut);
        entity.SetLowStock(request.LowStock);
        if (request.Notes is not null) entity.SetNotes(request.Notes);

        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public async Task CloseTodayAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var existing = await _db.DailyContexts
            .FirstOrDefaultAsync(d => d.Date == today, ct);

        if (existing is null)
        {
            existing = new DailyContext(_tenantContext.TenantId, today);
            _db.DailyContexts.Add(existing);
        }

        existing.CloseDay();
        await _db.SaveChangesAsync(ct);
    }

    private static DailyContextDto MapToDto(DailyContext entity) => new()
    {
        Id = entity.Id,
        Date = entity.Date,
        Weather = entity.Weather,
        DayType = entity.DayType,
        IsPayday = entity.IsPayday,
        HasLocalEvent = entity.HasLocalEvent,
        SchoolOut = entity.SchoolOut,
        LowStock = entity.LowStock,
        Notes = entity.Notes,
        ClosedAt = entity.ClosedAt,
        IsClosed = entity.IsClosed,
        CreatedAt = entity.CreatedAt,
    };
}
