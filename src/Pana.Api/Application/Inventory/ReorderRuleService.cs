namespace Pana.Api.Application.Inventory;

using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Common;
using Pana.Api.Domain.Inventory;
using Pana.Api.Infrastructure.Data;

public interface IReorderRuleService
{
    Task<List<ReorderRuleDto>> GetAllAsync(CancellationToken ct = default);
    Task<ReorderRuleDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReorderRuleDto> CreateAsync(CreateReorderRuleRequest request, CancellationToken ct = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default);
    Task<List<ReorderSuggestionDto>> GetReorderSuggestionsAsync(CancellationToken ct = default);
}

public class ReorderRuleService : IReorderRuleService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ReorderRuleService(PanaDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<ReorderRuleDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.ReorderRules
            .OrderBy(r => r.ProductId)
            .Select(r => MapToDto(r))
            .ToListAsync(ct);
    }

    public async Task<ReorderRuleDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await _db.ReorderRules.FindAsync([id], ct);
        return rule is null ? null : MapToDto(rule);
    }

    public async Task<ReorderRuleDto> CreateAsync(CreateReorderRuleRequest request, CancellationToken ct = default)
    {
        var rule = new ReorderRule(
            _tenantContext.TenantId,
            request.ProductId,
            request.LocationId,
            request.MinimumStock,
            request.MaximumStock,
            request.ReorderQuantity);

        _db.ReorderRules.Add(rule);
        await _db.SaveChangesAsync(ct);
        return MapToDto(rule);
    }

    public async Task<bool> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await _db.ReorderRules.FindAsync([id], ct);
        if (rule is null) return false;

        rule.Deactivate();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<ReorderSuggestionDto>> GetReorderSuggestionsAsync(CancellationToken ct = default)
    {
        var rules = await _db.ReorderRules
            .Where(r => r.IsActive)
            .ToListAsync(ct);

        var productIds = rules.Select(r => r.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        // Calculate current stock per product
        var stockLevels = await _db.InventoryMovements
            .GroupBy(m => m.ProductId)
            .Select(g => new { ProductId = g.Key, CurrentStock = g.Sum(m => m.Quantity) })
            .ToListAsync(ct);

        var stockDict = stockLevels.ToDictionary(s => s.ProductId, s => s.CurrentStock);

        var suggestions = new List<ReorderSuggestionDto>();
        foreach (var rule in rules)
        {
            var currentStock = stockDict.GetValueOrDefault(rule.ProductId, 0);
            if (rule.ShouldTrigger(currentStock) && products.TryGetValue(rule.ProductId, out var product))
            {
                suggestions.Add(new ReorderSuggestionDto(
                    rule.ProductId,
                    product.Name,
                    product.Sku,
                    currentStock,
                    rule.MinimumStock,
                    rule.MaximumStock - currentStock
                ));
            }
        }

        return suggestions;
    }

    private static ReorderRuleDto MapToDto(ReorderRule r) => new(
        r.Id, r.ProductId, r.LocationId, r.MinimumStock, r.MaximumStock, r.ReorderQuantity, r.IsActive, r.LastTriggeredAt
    );
}
