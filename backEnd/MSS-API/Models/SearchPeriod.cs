namespace MyApi.Models;

public static class SearchPeriod
{
    public const string Day = "Day";
    public const string Week = "Week";
    public const string Month = "Month";
    public const string All = "All";
}

public static class SearchPeriodHelper
{
    private static TimeSpan GetDuration(string period)
    {
        return period switch
        {
            SearchPeriod.Day => TimeSpan.FromDays(1),
            SearchPeriod.Week => TimeSpan.FromDays(7),
            SearchPeriod.Month => TimeSpan.FromDays(30),
            _ => TimeSpan.Zero
        };
    }

    public static DateTime? GetStartDate(string period)
    {
        if (period == SearchPeriod.All)
        {
            return null;
        }

        //検索には世界標準時刻（UTC)を使用
        return DateTime.UtcNow - GetDuration(period);
    }
}

