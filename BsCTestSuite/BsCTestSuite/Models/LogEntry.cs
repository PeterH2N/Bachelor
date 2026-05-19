using System.Text.Json.Serialization;

namespace BsCTestSuite.Models;

public class LogEntry
{
    [JsonPropertyName("@t")] public DateTime Timestamp { get; set; }
    [JsonPropertyName("@mt")] public string MessageTemplate { get; set; }
    [JsonPropertyName("@r")] public string[] Renderings { get; set; }
    [JsonPropertyName("@tr")] public string TraceId { get; set; }
    [JsonPropertyName("@sp")] public string SpanId { get; set; }
    [JsonPropertyName("RequestMethod")] public string RequestMethod { get; set; }
    [JsonPropertyName("RequestPath")] public string RequestPath { get; set; }
    [JsonPropertyName("StatusCode")] public int? StatusCode { get; set; }
    [JsonPropertyName("Elapsed")] public double? Elapsed { get; set; }
    [JsonPropertyName("SourceContext")] public string SourceContext { get; set; }
    [JsonPropertyName("CorrelationId")] public string CorrelationId { get; set; }
    [JsonPropertyName("RequestId")] public string RequestId { get; set; }
    [JsonPropertyName("ConnectionId")] public string ConnectionId { get; set; }
    [JsonPropertyName("@l")] public string Level { get; set; }
    [JsonPropertyName("@x")] public string Exception { get; set; }
    [JsonPropertyName("@m")] public string Message { get; set; }
}