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
