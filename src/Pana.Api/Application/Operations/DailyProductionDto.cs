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
    List<ProductionEventDto> Events,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// ── Event-based model (NEW — replaces line counters for UI) ──

/// <summary>
/// A single event in the daily production timeline.
/// </summary>
public record ProductionEventDto(
    Guid Id,
    Guid DailyProductionId,
    Guid ProductId,
    string ProductName,
    string EventType,
    decimal Quantity,
    string? Notes,
    Guid RegisteredByUserId,
    string RegisteredByUserName,
    DateTime CreatedAt
);

/// <summary>
/// Request to add a single production event (inicial, produccion, or devolucion).
/// </summary>
public record AddProductionEventRequest(
    Guid ProductId,
    string ProductName,
    string EventType,
    decimal Quantity,
    string? Notes
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
