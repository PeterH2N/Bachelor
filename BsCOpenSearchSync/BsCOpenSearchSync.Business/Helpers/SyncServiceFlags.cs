namespace BsCOpenSearchSync.Business.Helpers;

public static class SyncServiceFlags
{
    public static bool TestIsUnresponsive { get; set; }
    public static TimeSpan TestUnresponsiveDuration { get; set; }
    public static bool OpenSearchIsHealthy { get; set; } 
}