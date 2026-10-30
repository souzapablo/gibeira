namespace Gibeira.Web.Tests;

// Pins "today" for code that reads the date from a TimeProvider.
internal sealed class FixedTime(DateOnly today) : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => new(today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
}