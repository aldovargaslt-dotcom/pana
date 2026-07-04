namespace Pana.Api.Application.Common;

/// <summary>
/// DTOs for DailyContext — daily operational metadata that influences sales.
/// </summary>
public class DailyContextDto
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public string? Weather { get; init; }
    public string DayType { get; init; } = "normal";
    public bool IsPayday { get; init; }
    public bool HasLocalEvent { get; init; }
    public bool SchoolOut { get; init; }
    public bool LowStock { get; init; }
    public string? Notes { get; init; }
    public DateTime? ClosedAt { get; init; }
    public bool IsClosed { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class DailyContextUpsertRequest
{
    public DateTime Date { get; init; }
    public string? Weather { get; init; }
    public string? DayType { get; init; }
    public bool IsPayday { get; init; }
    public bool HasLocalEvent { get; init; }
    public bool SchoolOut { get; init; }
    public bool LowStock { get; init; }
    public string? Notes { get; init; }
}
