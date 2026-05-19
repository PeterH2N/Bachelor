using System.Text.Json;
using BsCTestSuite.Models;

namespace BsCTestSuite.Infrastructure;

public class LogReader(string logPath)
{
    public IEnumerable<LogEntry> Read()
    {
        var logDir = Path.GetDirectoryName(logPath)!;
        var pattern = Path.GetFileName(logPath); // e.g. "log-*.json"
    
        return Directory.GetFiles(logDir, pattern)
            .OrderBy(f => f)
            .SelectMany(File.ReadLines)
            .Select(line => JsonSerializer.Deserialize<LogEntry>(line))
            .Where(e => e != null)!;
    }

    public IEnumerable<LogEntry> ForCorrelation(string correlationId) =>
        Read().Where(e => e.CorrelationId == correlationId);
}