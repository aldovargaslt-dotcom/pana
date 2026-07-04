namespace Pana.Api.Domain.Common;

/// <summary>
/// Captures daily operational context that may influence sales patterns.
/// Weather, day type, payday cycles, local events — all factors that correlate
/// with bakery foot traffic and purchasing behavior.
/// Extracted from real bakery operations knowledge.
/// </summary>
public class DailyContext : TenantEntity
{
    public DateTime Date { get; private set; }
    public string? Weather { get; private set; }
    public string DayType { get; private set; }
    public bool IsPayday { get; private set; }
    public bool HasLocalEvent { get; private set; }
    public bool SchoolOut { get; private set; }
    public bool LowStock { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    public static class Weathers
    {
        public const string Frio = "frio";
        public const string Templado = "templado";
        public const string Calor = "calor";
        public const string Lluvia = "lluvia";
    }

    public static class DayTypes
    {
        public const string Normal = "normal";
        public const string Finde = "finde";
        public const string Festivo = "festivo";
        public const string Vacaciones = "vacaciones";
    }

    private DailyContext() { } // EF Core

    public DailyContext(Guid tenantId, DateTime date, string dayType = DayTypes.Normal)
        : base(tenantId)
    {
        if (date == default)
            throw new ArgumentException("Date is required.", nameof(date));
        if (dayType is not (DayTypes.Normal or DayTypes.Finde or DayTypes.Festivo or DayTypes.Vacaciones))
            throw new ArgumentException($"Invalid day type: {dayType}.", nameof(dayType));

        Date = date.Date;
        DayType = dayType;
    }

    public void SetWeather(string? weather)
    {
        if (weather is not null && weather is not (Weathers.Frio or Weathers.Templado or Weathers.Calor or Weathers.Lluvia))
            throw new ArgumentException($"Invalid weather: {weather}.", nameof(weather));
        Weather = weather;
        MarkUpdated();
    }

    public void SetDayType(string dayType)
    {
        if (dayType is not (DayTypes.Normal or DayTypes.Finde or DayTypes.Festivo or DayTypes.Vacaciones))
            throw new ArgumentException($"Invalid day type: {dayType}.", nameof(dayType));
        DayType = dayType;
        MarkUpdated();
    }

    public void SetIsPayday(bool isPayday) { IsPayday = isPayday; MarkUpdated(); }
    public void SetHasLocalEvent(bool hasEvent) { HasLocalEvent = hasEvent; MarkUpdated(); }
    public void SetSchoolOut(bool schoolOut) { SchoolOut = schoolOut; MarkUpdated(); }
    public void SetLowStock(bool lowStock) { LowStock = lowStock; MarkUpdated(); }

    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
        MarkUpdated();
    }

    public void CloseDay()
    {
        if (ClosedAt is not null)
            return;
        ClosedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public bool IsClosed => ClosedAt is not null;
}
