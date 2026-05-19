using BsCTestSuite.Models;

namespace BsCTestSuite.Infrastructure;

public class TestFixture : IAsyncLifetime
{
    public HttpClient CaseClient { get; private set; }
    public HttpClient SyncClient { get; private set; }
    public LogReader CaseLogReader { get; private set; }
    public LogReader SyncLogReader { get; private set; }

    public Task InitializeAsync()
    {
        CaseClient = new HttpClient { BaseAddress = new Uri("http://localhost:5250") };
        SyncClient = new HttpClient { BaseAddress = new Uri("http://localhost:5142") };
        CaseLogReader = new LogReader("../../../../../BscCaseApi/logs/log-*.json");
        SyncLogReader = new LogReader("../../../../../BscOpenSearchSync/logs/log-*.json");
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        CaseClient.Dispose();
        return Task.CompletedTask;
    }

    public IEnumerable<LogEntry> ReadLogs()
    {
        return CaseLogReader.Read().Union(SyncLogReader.Read()).OrderBy(e => e.Timestamp);
    }
    
    public IEnumerable<LogEntry> LogsForCorrelationId(string correlationId)
    {
        return ReadLogs().Where(e => e.CorrelationId == correlationId);
    }
}