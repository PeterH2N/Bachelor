using System.Collections.Concurrent;
using BsCTestSuite.Infrastructure;
using Xunit.Abstractions;

namespace BsCTestSuite.Tests;

[Collection("Tests")]
public class PerformanceTests(TestFixture fixture, ITestOutputHelper output) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task RequestsShouldCompleteWithin1Second()
    {
        var response = await fixture.CaseClient.PutAsync("/api/Case/CreateRandom?amount=1", null);
        var correlationId = response.Headers.GetValues("X-Correlation-Id").First();

        // wait for sync to complete
        await Task.Delay(5000);

        var logs = fixture.LogsForCorrelationId(correlationId).ToList();
        var start = logs.Min(e => e.Timestamp);
        var end = logs.Max(e => e.Timestamp);
            
        var time = (end - start).TotalMilliseconds;
        
        output.WriteLine("Test complete");
        output.WriteLine($"Time taken: {time}ms");
        
        Assert.True(time < 1000);
        
    }
}