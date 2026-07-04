namespace Pana.Api.Web.Components;

public record StatCardProps(
    string Label,
    string Value,
    string? Trend = null,      // "up" | "down" | null
    string? TrendValue = null, // "+12%" or "-5%"
    string? Icon = null,
    string? HxGet = null
);
