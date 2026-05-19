namespace BsCOpenSearchSync.Models;

public static class SyncServiceFlags
{
    public static bool IsUnresponsive { get; set; }
    public static TimeSpan UnresponsiveDuration { get; set; }
}