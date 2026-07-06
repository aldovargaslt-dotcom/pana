namespace Pana.Api.Application.Analytics;

/// <summary>
/// DTOs for business analytics: P&L, waste analysis, sales trends.
/// </summary>

/// <summary>Profit & Loss summary for a date range.</summary>
public record PlSummaryDto(
    decimal TotalIngresos,
    decimal TotalCOGS,
    decimal UtilidadBruta,
    decimal MargenBrutoPct,
    List<PlProductBreakdownDto> ProductBreakdown,
    List<PlDailySaleDto> DailySales,
    DateTime From,
    DateTime To
);

public record PlProductBreakdownDto(
    Guid ProductId,
    string ProductName,
    int UnidadesVendidas,
    decimal Ingreso,
    decimal COGSTotal,
    decimal COGSPorUnidad,
    decimal Margen,
    decimal MargenPct
);

public record PlDailySaleDto(
    DateTime Date,
    string Label,
    decimal Ingreso,
    int Transacciones
);

/// <summary>Waste/merma analysis for a date range.</summary>
public record WasteAnalysisDto(
    decimal TotalProducido,
    decimal TotalMerma,
    decimal EficienciaPct,
    decimal CostoTotalMerma,
    List<WasteProductStatDto> Products,
    DateTime From,
    DateTime To
);

public record WasteProductStatDto(
    Guid ProductId,
    string ProductName,
    decimal Producido,
    decimal Merma,
    decimal MermaPct,
    decimal CostoPorUnidad,
    decimal CostoTotalMerma,
    string? TopWasteCategory
);

/// <summary>Sales trends summary.</summary>
public record SalesTrendsDto(
    decimal VentasTotales,
    int TotalTransacciones,
    decimal TicketPromedio,
    List<PaymentMethodBreakdownDto> PaymentMethods,
    List<DailyTrendPointDto> DailyTrend,
    DateTime From,
    DateTime To
);

public record PaymentMethodBreakdownDto(
    string Method,
    int Count,
    decimal Total
);

public record DailyTrendPointDto(
    DateTime Date,
    string Label,
    decimal Ventas,
    int Transacciones,
    decimal TicketPromedio
);

/// <summary>BCG Matrix — product portfolio classification (Stars, Cash Cows, Question Marks, Dogs).</summary>
public record BcgMatrixDto(
    List<BcgProductDto> Products,
    DateTime From,
    DateTime To,
    decimal AvgSales,
    decimal AvgGrowthPct
);

public record BcgProductDto(
    Guid ProductId,
    string ProductName,
    decimal UnitsSold,
    decimal Revenue,
    decimal RelativeShare,
    decimal GrowthPct,
    string Quadrant,     // "Star", "CashCow", "QuestionMark", "Dog"
    string Strategy       // Human-readable recommendation
)
{
    public static string DeriveQuadrant(decimal relativeShare, decimal growthPct, decimal avgGrowth)
    {
        if (relativeShare >= 1.0m && growthPct >= avgGrowth) return "Star";
        if (relativeShare >= 1.0m && growthPct < avgGrowth) return "CashCow";
        if (relativeShare < 1.0m && growthPct >= avgGrowth) return "QuestionMark";
        return "Dog";
    }

    public static string DeriveStrategy(string quadrant) => quadrant switch
    {
        "Star" => "Invertir y promover — alto crecimiento y alta participación.",
        "CashCow" => "Mantener y capitalizar — genera utilidades estables.",
        "QuestionMark" => "Evaluar — tiene potencial pero poca participación aún.",
        "Dog" => "Considerar descontinuar o reposicionar.",
        _ => "Sin clasificación."
    };
}
