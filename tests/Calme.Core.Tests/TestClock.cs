namespace Calme.Core.Tests;

/// <summary>Relógio controlado pelos testes: avança o tempo monotônico e define "agora" e o fuso local.</summary>
sealed class TestClock(DateTimeOffset? now = null, TimeZoneInfo? zone = null) : TimeProvider
{
    private long ticks;
    public DateTimeOffset Now { get; set; } = now ?? new DateTimeOffset(2026, 10, 22, 12, 0, 0, TimeSpan.Zero);
    public override TimeZoneInfo LocalTimeZone { get; } = zone ?? TimeZoneInfo.Utc;
    public override DateTimeOffset GetUtcNow() => Now;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => ticks;
    public void Advance(double seconds) => ticks += (long)(seconds * TimeSpan.TicksPerSecond);
}
