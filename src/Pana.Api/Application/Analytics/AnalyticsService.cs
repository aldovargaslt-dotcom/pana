using Microsoft.EntityFrameworkCore;
using Pana.Api.Domain.Production;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Analytics;

public interface IAnalyticsService
{
    Task<PlSummaryDto> GetProfitLossAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<WasteAnalysisDto> GetWasteAnalysisAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<SalesTrendsDto> GetSalesTrendsAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly PanaDbContext _db;

    public AnalyticsService(PanaDbContext db)
    {
        _db = db;
    }

    public async Task<PlSummaryDto> GetProfitLossAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1); // inclusive end

        // ── Fetch completed (delivered) sales in range ──
        var sales = await _db.Sales
            .Where(s => s.Status == Domain.Sales.Sale.Statuses.Delivered
                     && s.CreatedAt >= fromDate
                     && s.CreatedAt < toDate)
            .Include(s => s.Items)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        // ── Fetch recipes with their product links and ingredients ──
        var recipeProductMap = await _db.Recipes
            .Where(r => r.ProductId != null && r.IsActive)
            .ToDictionaryAsync(r => r.ProductId!.Value, ct);

        // ── Fetch all raw materials for cost calculation ──
        var materialCostMap = await _db.RawMaterials
            .Where(m => m.IsActive)
            .ToDictionaryAsync(m => m.Id, m => m.CostPerBaseUnit, ct);

        // ── Build recipe cost map ──
        var recipeCostMap = new Dictionary<Guid, decimal>(); // recipeId → costPerUnit
        var allRecipes = await _db.Recipes
            .Where(r => r.IsActive)
            .Include(r => r.Ingredients)
            .ToListAsync(ct);

        foreach (var recipe in allRecipes)
        {
            var rawCost = recipe.Ingredients.Sum(i =>
            {
                if (!materialCostMap.TryGetValue(i.MaterialId, out var materialCost))
                    return 0m;
                return UnitConversion.CalculateIngredientCost(i.Qty, i.Unit, "g", materialCost); // simplified — uses g as base
            });
            var totalCost = rawCost + (recipe.LaborCostPerUnit * recipe.Yield) + recipe.EnergyCost
                          + (rawCost * recipe.OverheadPct / 100m);
            var costPerUnit = recipe.Yield > 0 ? totalCost / recipe.Yield : 0;
            recipeCostMap[recipe.Id] = costPerUnit;
        }

        // ── Aggregate by product ──
        var productMap = new Dictionary<Guid, (string Name, int Qty, decimal Ingreso, decimal COGS)>();
        foreach (var sale in sales)
        {
            foreach (var item in sale.Items.Where(i => !i.IsVoided))
            {
                if (!productMap.ContainsKey(item.ProductId))
                    productMap[item.ProductId] = (item.ProductName, 0, 0, 0);

                var cogsPerUnit = 0m;
                if (recipeProductMap.TryGetValue(item.ProductId, out var recipe))
                    recipeCostMap.TryGetValue(recipe.Id, out cogsPerUnit);

                var entry = productMap[item.ProductId];
                productMap[item.ProductId] = (
                    entry.Name,
                    entry.Qty + item.Quantity,
                    entry.Ingreso + item.LineTotal,
                    entry.COGS + (cogsPerUnit * item.Quantity)
                );
            }
        }

        var totalIngresos = sales.Sum(s => s.TotalAmount);
        var totalCOGS = productMap.Values.Sum(p => p.COGS);
        var utilidadBruta = totalIngresos - totalCOGS;
        var margenBrutoPct = totalIngresos > 0 ? (utilidadBruta / totalIngresos) * 100 : 0;

        var productBreakdown = productMap
            .Select(kvp => new PlProductBreakdownDto(
                kvp.Key,
                kvp.Value.Name,
                kvp.Value.Qty,
                kvp.Value.Ingreso,
                kvp.Value.COGS,
                kvp.Value.Qty > 0 ? kvp.Value.COGS / kvp.Value.Qty : 0,
                kvp.Value.Ingreso - kvp.Value.COGS,
                kvp.Value.Ingreso > 0 ? ((kvp.Value.Ingreso - kvp.Value.COGS) / kvp.Value.Ingreso) * 100 : 0
            ))
            .OrderByDescending(p => p.Ingreso)
            .ToList();

        var dailySales = sales
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new PlDailySaleDto(
                g.Key,
                g.Key.ToString("ddd dd/MM"),
                g.Sum(s => s.TotalAmount),
                g.Count()
            ))
            .OrderBy(d => d.Date)
            .ToList();

        return new PlSummaryDto(totalIngresos, totalCOGS, utilidadBruta, margenBrutoPct, productBreakdown, dailySales, from, to);
    }

    public async Task<WasteAnalysisDto> GetWasteAnalysisAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        // ── Production in (what was made) ──
        var productionMovements = await _db.InventoryMovements
            .Where(m => m.MovementType == Domain.Inventory.InventoryMovement.Types.ProductionIn
                     && m.CreatedAt >= fromDate && m.CreatedAt < toDate)
            .GroupBy(m => m.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(m => m.Quantity) })
            .ToListAsync(ct);

        // ── Waste out (what was wasted) ──
        var wasteMovements = await _db.InventoryMovements
            .Where(m => m.MovementType == Domain.Inventory.InventoryMovement.Types.ProductionOut
                     && m.CreatedAt >= fromDate && m.CreatedAt < toDate)
            .ToListAsync(ct);

        // ── Product lookup ──
        var allProductIds = productionMovements.Select(p => p.ProductId)
            .Concat(wasteMovements.Select(w => w.ProductId))
            .Distinct()
            .ToList();
        var products = await _db.Products
            .Where(p => allProductIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        // ── Waste category lookup ──
        var wasteCategoryIds = wasteMovements
            .Where(w => w.WasteCategoryId.HasValue)
            .Select(w => w.WasteCategoryId!.Value)
            .Distinct()
            .ToList();
        var wasteCategories = await _db.WasteCategories
            .Where(w => wasteCategoryIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.Name, ct);

        var prodByProduct = productionMovements.ToDictionary(p => p.ProductId, p => p.Total);
        var wasteByProduct = wasteMovements
            .GroupBy(w => w.ProductId)
            .ToDictionary(g => g.Key, g => new
            {
                Total = g.Sum(w => Math.Abs(w.Quantity)),
                TopCategoryId = g.Where(w => w.WasteCategoryId.HasValue)
                    .GroupBy(w => w.WasteCategoryId!.Value)
                    .OrderByDescending(x => x.Sum(w => Math.Abs(w.Quantity)))
                    .FirstOrDefault()?.Key
            });

        var allIds = new HashSet<Guid>(prodByProduct.Keys);
        foreach (var id in wasteByProduct.Keys) allIds.Add(id);

        var totalProducido = prodByProduct.Values.Sum();
        var totalMerma = wasteByProduct.Values.Sum(w => w.Total);

        var productStats = allIds
            .Select(id =>
            {
                products.TryGetValue(id, out var product);
                prodByProduct.TryGetValue(id, out var prod);
                var waste = wasteByProduct.GetValueOrDefault(id);
                var mermaQty = waste?.Total ?? 0;
                var costoPorUnidad = product?.Cost ?? 0;
                var mermaPct = (prod + mermaQty) > 0 ? mermaQty / (prod + mermaQty) * 100 : 0;
                var topCategory = waste?.TopCategoryId is { } catId && wasteCategories.TryGetValue(catId, out var catName)
                    ? catName : null;

                return new WasteProductStatDto(
                    id,
                    product?.Name ?? "(desconocido)",
                    prod,
                    mermaQty,
                    Math.Round(mermaPct, 1),
                    costoPorUnidad,
                    mermaQty * costoPorUnidad,
                    topCategory
                );
            })
            .OrderByDescending(p => p.Merma)
            .ToList();

        var eficiencia = totalProducido > 0 ? ((totalProducido - totalMerma) / totalProducido) * 100 : 0;
        var costoTotalMerma = productStats.Sum(p => p.CostoTotalMerma);

        return new WasteAnalysisDto(totalProducido, totalMerma, Math.Round(eficiencia, 1), costoTotalMerma, productStats, from, to);
    }

    public async Task<SalesTrendsDto> GetSalesTrendsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        var sales = await _db.Sales
            .Where(s => s.Status == Domain.Sales.Sale.Statuses.Delivered
                     && s.CreatedAt >= fromDate && s.CreatedAt < toDate)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        var ventasTotales = sales.Sum(s => s.TotalAmount);
        var totalTransacciones = sales.Count;
        var ticketPromedio = totalTransacciones > 0 ? ventasTotales / totalTransacciones : 0;

        // Payment methods — note: Sale entity doesn't have PaymentMethod yet,
        // so we default to "Efectivo" until that field is added
        var paymentMethods = new List<PaymentMethodBreakdownDto>
        {
            new("General", sales.Count, ventasTotales)
        };

        var dailyTrend = sales
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new DailyTrendPointDto(
                g.Key,
                g.Key.ToString("ddd dd/MM"),
                g.Sum(s => s.TotalAmount),
                g.Count(),
                g.Count() > 0 ? g.Sum(s => s.TotalAmount) / g.Count() : 0
            ))
            .OrderBy(d => d.Date)
            .ToList();

        return new SalesTrendsDto(ventasTotales, totalTransacciones, ticketPromedio, paymentMethods, dailyTrend, from, to);
    }
}
