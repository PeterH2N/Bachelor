using System.Diagnostics;
using System.Net;
using System.Text.Json;
using BsCTestSuite.Infrastructure;

namespace BsCTestSuite.Tests;

public class ReliabilityTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    // check count of a certain index, after adding 100 requests, the count should be 100 higher
    [Fact]
    public async Task NoRequestsShouldBeDroppedUnder100ConcurrentRequests()
    {
        var oldCountRes = await fixture.SyncClient.GetAsync("/api/Stats/GetCount/cases");
        var json = await oldCountRes.Content.ReadAsStringAsync();
        var oldCount = JsonDocument.Parse(json).RootElement.GetProperty("count").GetInt32();
        
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => fixture.CaseClient.PutAsync("/api/Case/CreateRandom?amount=1", null));

        var responses = await Task.WhenAll(tasks);
        
        await Task.Delay(5000); // wait for all syncs
        
        var newCountRes = await fixture.SyncClient.GetAsync("/api/Stats/GetCount/cases");
        json = await newCountRes.Content.ReadAsStringAsync();
        var newCount = JsonDocument.Parse(json).RootElement.GetProperty("count").GetInt32();

        Assert.Equal(oldCount + 100, newCount);
    }
    
    [Fact]
    public async Task CaseApi_ShouldHandleSyncApiDown()
    {
        var oldCountRes = await fixture.SyncClient.GetAsync("/api/Stats/GetCount/cases");
        var json = await oldCountRes.Content.ReadAsStringAsync();
        var oldCount = JsonDocument.Parse(json).RootElement.GetProperty("count").GetInt32();
        
        await fixture.SyncClient.PostAsync("api/Test/SimulateShutdown", null);
        await Task.Delay(1000);

        var tasks = Enumerable.Range(0, 100)
            .Select(_ => fixture.CaseClient.PutAsync("/api/Case/CreateRandom?amount=1", null));

        var responses = await Task.WhenAll(tasks);

        await RestartSyncApi();
        
        await Task.Delay(5000);
        
        var newCountRes = await fixture.SyncClient.GetAsync("/api/Stats/GetCount/cases");
        json = await newCountRes.Content.ReadAsStringAsync();
        var newCount = JsonDocument.Parse(json).RootElement.GetProperty("count").GetInt32();
        
        Assert.Equal(oldCount + 100, newCount);
    }
    
    private async Task RestartSyncApi()
    {
        var solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var projectPath = Path.Combine(solutionDir, "BsCOpenSearchSync", "BsCOpenSearchSync.Api", "BsCOpenSearchSync.Api.csproj");
        
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project {projectPath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[SYNC] {e.Data}");
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[SYNC ERR] {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    
        // wait for it to be responsive again
        await WaitForApi(fixture.SyncClient);
    }

    private static async Task WaitForApi(HttpClient client, int timeoutSeconds = 30)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch { /* not up yet */ }
        
            await Task.Delay(500);
        }
    
        throw new TimeoutException("Sync API did not restart in time");
    }
}