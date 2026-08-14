namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Abstraction for system clock to enable testing and timezone consistency.
/// All DateTime.UtcNow usage should go through this interface.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time based on the configured timezone.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets today's date in UTC.
    /// </summary>
    DateTime Today { get; }
}

/// <summary>
/// Default implementation using system clock.
/// Replace with a mock in tests or a configurable implementation for different timezones.
/// </summary>
public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateTime Today => DateTime.UtcNow.Date;
}

/// <summary>
/// Frozen clock for testing purposes.
/// </summary>
public class FrozenClock : IClock
{
    private readonly DateTime _frozenTime;

    public FrozenClock(DateTime frozenTime)
    {
        _frozenTime = frozenTime.Kind == DateTimeKind.Utc
            ? frozenTime
            : frozenTime.ToUniversalTime();
    }

    public DateTime UtcNow => _frozenTime;
    public DateTime Now => _frozenTime.ToLocalTime();
    public DateTime Today => _frozenTime.Date;
}
