namespace Pana.Api.Web.ViewModels;

public record DashboardViewModel(
    string TenantName,
    List<DashboardWidgetViewModel> Widgets,
    DashboardKpiViewModel Kpi
);

public record DashboardWidgetViewModel(
    string Id,
    string WidgetType,
    string Title,
    string HxEndpoint,
    int Width
);

public record DashboardKpiViewModel(
    decimal TodayRevenue,
    int TodayOrderCount,
    decimal TodayMargin,
    decimal YesterdayRevenue,
    int YesterdayOrderCount,
    int ActiveProductCount,
    int LowStockCount
);

public record SalesChartData(
    string Day,
    decimal Total
);

public record LowStockAlertViewModel(
    Guid ProductId,
    string ProductName,
    decimal StockLevel,
    decimal MinimumLevel
);
