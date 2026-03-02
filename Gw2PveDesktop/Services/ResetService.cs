namespace Gw2PveDesktop.Services;

/// <summary>
/// GW2 daily reset is at midnight UTC.
/// See: ResetsWatcherService in BlishHud-Raid-Clears.
/// </summary>
public static class ResetService
{
    /// <summary>
    /// Next daily reset (midnight UTC).
    /// </summary>
    public static DateTime NextDailyReset
    {
        get
        {
            var now = DateTime.UtcNow;
            var tomorrow = now.Date.AddDays(1);
            return tomorrow;
        }
    }

    /// <summary>
    /// Time remaining until next daily reset.
    /// </summary>
    public static TimeSpan TimeUntilReset => NextDailyReset - DateTime.UtcNow;

    /// <summary>
    /// Formatted countdown string, e.g. "2h 34m 12s".
    /// </summary>
    public static string GetCountdownString()
    {
        var remaining = TimeUntilReset;
        if (remaining.TotalSeconds <= 0)
            return "Resetting...";

        var parts = new List<string>();
        if (remaining.Hours > 0)
            parts.Add($"{remaining.Hours}h");
        parts.Add($"{remaining.Minutes}m");
        parts.Add($"{remaining.Seconds}s");
        return string.Join(" ", parts);
    }
}
