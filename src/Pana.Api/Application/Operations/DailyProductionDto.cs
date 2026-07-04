namespace Pana.Api.Application.Operations;

/// <summary>
/// DTOs for daily production capture — the shop-floor operational core.
/// </summary>
public record DailyProductionDto(
    Guid Id,
    Guid DailyContextId,
    DateTime Date,
    bool IsClosed,
    DateTime? ClosedAt,
    List<DailyProductionLineDto> Lines,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record DailyProductionLineDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Inicial,
    decimal Produccion,
    decimal Devolucion,
    decimal Disponible
);

/// <summary>
/// Request to create or update today's production capture.
/// </summary>
public record UpsertDailyProductionRequest(
    List<DailyProductionLineRequest> Lines
);

public record DailyProductionLineRequest(
    Guid ProductId,
    string ProductName,
    decimal Inicial,
    decimal Produccion,
    decimal Devolucion
);
