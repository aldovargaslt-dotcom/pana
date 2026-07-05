namespace Pana.Api.Web.ViewModels;

// ── POS View Models ─────────────────────────────────────────────

public record PosViewModel(
    List<PosCategoryViewModel> Categories,
    List<PosProductCardViewModel> Products,
    PosActiveOrderViewModel? ActiveOrder
);

public record PosCategoryViewModel(
    string Name,
    string Icon,
    int ProductCount
);

public record PosProductCardViewModel(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string ProductType,
    bool IsActive
);

public record PosActiveOrderViewModel(
    Guid? SaleId,
    string? OrderLabel,
    string? TableNumber,
    string OrderType,
    List<PosOrderItemViewModel> Items,
    decimal Subtotal,
    decimal Tax,
    decimal Discount,
    decimal Total,
    string? PromoCode,
    string PaymentMethod
);

public record PosOrderItemViewModel(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);

public record PosProductDetailViewModel(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string ProductType,
    bool IsActive
);

// ── Activity View Models ────────────────────────────────────────

public record ActivityViewModel(
    string ActiveTab,
    List<BillingQueueItemViewModel> QueueItems,
    List<OrderHistoryItemViewModel> OrderHistory,
    int ActiveCount,
    int ClosedCount
);

public record BillingQueueItemViewModel(
    Guid Id,
    string OrderLabel,
    string? TableNumber,
    DateTime CreatedAt,
    decimal TotalAmount,
    string Status
);

public record OrderHistoryItemViewModel(
    Guid Id,
    int Sequence,
    DateTime CreatedAt,
    string OrderLabel,
    string Status,
    decimal TotalAmount,
    string PaymentStatus
);

// ── Reports View Models ─────────────────────────────────────────

public record ReportsViewModel(
    string PeriodLabel,
    DateTime From,
    DateTime To,
    ReportsMetricsViewModel Metrics,
    List<DailyTrendPointViewModel> DailyTrend,
    List<TopProductViewModel> TopProducts,
    List<OrderHistoryItemViewModel> AllOrders
);

public record ReportsMetricsViewModel(
    decimal TotalSales,
    int TotalTransactions,
    int TotalCustomers,
    decimal NetProfit,
    decimal SalesGrowthPct,
    decimal TransactionGrowthPct,
    decimal CustomerGrowthPct,
    decimal ProfitGrowthPct
);

public record DailyTrendPointViewModel(
    DateTime Date,
    string Label,
    decimal Ventas,
    int Transacciones
);

public record TopProductViewModel(
    Guid ProductId,
    string ProductName,
    string Category,
    int TotalOrders,
    decimal TotalSold
);
