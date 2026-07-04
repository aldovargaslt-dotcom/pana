namespace Pana.Api.Application.Inventory;

using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Common;
using Pana.Api.Domain.Inventory;
using Pana.Api.Infrastructure.Data;

public interface IStockLocationService
{
    Task<List<StockLocationDto>> GetAllAsync(CancellationToken ct = default);
    Task<StockLocationDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<StockLocationDto> CreateAsync(CreateStockLocationRequest request, CancellationToken ct = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default);
}

public class StockLocationService : IStockLocationService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public StockLocationService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<StockLocationDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.StockLocations
            .OrderBy(l => l.Name)
            .Select(l => MapToDto(l))
            .ToListAsync(ct);
    }

    public async Task<StockLocationDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var location = await _db.StockLocations.FindAsync([id], ct);
        return location is null ? null : MapToDto(location);
    }

    public async Task<StockLocationDto> CreateAsync(CreateStockLocationRequest request, CancellationToken ct = default)
    {
        var location = new StockLocation(
            _tenantContext.TenantId,
            request.Name,
            request.Code,
            request.LocationType,
            request.ParentLocationId);

        _db.StockLocations.Add(location);
        await _db.SaveChangesAsync(ct);
        return MapToDto(location);
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var location = await _db.StockLocations.FindAsync([id], ct);
        if (location is null) return false;

        location.Deactivate();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static StockLocationDto MapToDto(StockLocation l) => new(
        l.Id, l.Name, l.Code, l.LocationType, l.ParentLocationId, l.IsActive
    );
}
