namespace Pana.Api.Web.ViewModels;

public record DashboardViewModel(
    string TenantName,
    List<DashboardWidgetViewModel> Widgets
);

public record DashboardWidgetViewModel(
    string Id,
    string WidgetType,
    string Title,
    string HxEndpoint,
    int Width
);

public record SalesChartData(
    string Day,
    decimal Total
);
