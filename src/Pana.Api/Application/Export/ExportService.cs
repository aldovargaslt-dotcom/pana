using System.Text;
using Microsoft.EntityFrameworkCore;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Export;

public interface IExportService
{
    Task<byte[]> ExportSalesCsvAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<byte[]> ExportMaterialsCsvAsync(CancellationToken ct = default);
    Task<byte[]> ExportPlCsvAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public class ExportService : IExportService
{
    private readonly PanaDbContext _db;
    private readonly Analytics.IAnalyticsService _analytics;

    public ExportService(PanaDbContext db, Analytics.IAnalyticsService analytics)
    {
        _db = db;
        _analytics = analytics;
    }

    public async Task<byte[]> ExportSalesCsvAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1);

        var sales = await _db.Sales
            .Where(s => s.CreatedAt >= fromDate && s.CreatedAt < toDate)
            .Include(s => s.Items)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Fecha,Venta ID,Producto,Cantidad,Precio Unit,Subtotal,Estado");

        foreach (var sale in sales)
        {
            foreach (var item in sale.Items)
            {
                sb.AppendLine($"{sale.CreatedAt:yyyy-MM-dd},{sale.Id},{EscapeCsv(item.ProductName)},{item.Quantity},{item.UnitPrice:F2},{item.LineTotal:F2},{sale.Status}");
            }
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportMaterialsCsvAsync(CancellationToken ct = default)
    {
        var materials = await _db.RawMaterials
            .Where(m => m.IsActive)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Nombre,Categoría,Unidad Compra,Precio Compra,Presentación,Unidad Base,Rendimiento %,Costo/Unidad Base,Proveedor");

        foreach (var m in materials)
        {
            sb.AppendLine($"{EscapeCsv(m.Name)},{EscapeCsv(m.Category)},{m.PurchaseUnit},{m.PurchasePrice:F4},{m.PresentationQty:F4},{m.BaseUnit},{m.YieldPct:F1},{m.CostPerBaseUnit:F6},{EscapeCsv(m.Supplier ?? "")}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportPlCsvAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var pl = await _analytics.GetProfitLossAsync(from, to, ct);

        var sb = new StringBuilder();
        sb.AppendLine("--- P&L RESUMEN ---");
        sb.AppendLine($"Ingresos Totales,{pl.TotalIngresos:F2}");
        sb.AppendLine($"COGS Total,{pl.TotalCOGS:F2}");
        sb.AppendLine($"Utilidad Bruta,{pl.UtilidadBruta:F2}");
        sb.AppendLine($"Margen Bruto %,{pl.MargenBrutoPct:F1}");
        sb.AppendLine();
        sb.AppendLine("--- DESGLOSE POR PRODUCTO ---");
        sb.AppendLine("Producto,Unidades,Ingreso,COGS Unit,COGS Total,Margen,Margen %");

        foreach (var p in pl.ProductBreakdown)
        {
            sb.AppendLine($"{EscapeCsv(p.ProductName)},{p.UnidadesVendidas},{p.Ingreso:F2},{p.COGSPorUnidad:F4},{p.COGSTotal:F2},{p.Margen:F2},{p.MargenPct:F1}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
