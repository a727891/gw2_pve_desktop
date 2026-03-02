namespace Gw2PveDesktop.Services;

/// <summary>
/// 366-day cycle with Feb 29 skipped on non-leap years.
/// See: https://wiki.guildwars2.com/wiki/Template:Day_of_year_index
/// </summary>
public static class DayOfYearIndexService
{
    /// <summary>
    /// Returns 0-365. On non-leap years, index 59 (Feb 29) is never used.
    /// </summary>
    public static int DayOfYearIndex(DateTime date)
    {
        var day = date.DayOfYear - 1; // 0-based
        if (DateTime.IsLeapYear(date.Year))
            return day;
        if (date.Month >= 3)
            return day + 1; // skip index 59 on non-leap
        return day;
    }

    public static int DayOfYearIndex() => DayOfYearIndex(DateTime.UtcNow);
}
