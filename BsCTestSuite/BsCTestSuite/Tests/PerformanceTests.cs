using BsCTestSuite.Infrastructure;

namespace BsCTestSuite.Tests;

public class PerformanceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task RequestsShouldCompleteWithin800Milliseconds()
    {
        var response = await fixture.CaseClient.PutAsync("/api/Case/CreateRandom?amount=1", null);
        var correlationId = response.Headers.GetValues("X-Correlation-Id").First();

        // wait for sync to complete
        await Task.Delay(3000);

        var logs = fixture.LogsForCorrelationId(correlationId).ToList();
        var start = logs.Min(e => e.Timestamp);
        var end = logs.Max(e => e.Timestamp);

        Assert.True((end - start).TotalMilliseconds < 800);
    }
}