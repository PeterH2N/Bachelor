using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using BsCTestSuite.Infrastructure;
using Xunit.Abstractions;

namespace BsCTestSuite.Tests;

[Collection("Tests")]
public class ChaosTest(TestFixture fixture, ITestOutputHelper output) : IClassFixture<TestFixture>
{
    private static readonly TimeSpan TestDuration        = TimeSpan.FromMinutes(2);
    private static readonly int RequestsPerMinute = 1000;
    private static readonly TimeSpan RequestInterval = TimeSpan.FromMilliseconds(60_000.0 / RequestsPerMinute * 10);
    private static readonly TimeSpan DesyncCheckInterval = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan RestartInterval     = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DesyncPauseDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan RestartDuration     = TimeSpan.FromSeconds(10);
    private static readonly int UpdatesPerCreates = 4;
    
    private static readonly int IntervalJitterMs         = 0; // ± jitter on each interval
    
    private static readonly Random Rng = new();
    
    [Fact]
    public async Task RunChaosTest()
    {
        using var cts = new CancellationTokenSource(TestDuration);
        var ct = cts.Token;

        int requestCount   = 0;
        int desyncChecks   = 0;
        int desyncCount    = 0;
        int desyncDocs     = 0;
        int restartCount   = 0;
        int errorCount     = 0;

        // Gate held by the desync checker to pause request sending before each check.
        var pauseGate = new SemaphoreSlim(1, 1);
        var restartGate = new SemaphoreSlim(1, 1);

        var tasks = new[]
        {
            SendRandomRequestsAsync(),
            CheckForDesyncsAsync(),
            RestartSyncServiceAsync()
        };

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // expected — test duration elapsed
        }

        var avg = (double)desyncDocs / desyncChecks;

        output.WriteLine("─── Chaos Test Complete ───────────────────────");
        output.WriteLine($"  Requests sent          : {requestCount}");
        output.WriteLine($"  Desync checks          : {desyncChecks} checks, {desyncCount} desyncs found");
        output.WriteLine($"  Average desynced docs  : {avg}");
        output.WriteLine($"  Restarts               : {restartCount}");
        output.WriteLine($"  Errors                 : {errorCount}");
        output.WriteLine("───────────────────────────────────────────────");
        
        await Task.Delay(1000); // Give all syncs the chance to complete

        Assert.Equal(0, await AmountOfDeltaDocs());

        async Task SendRandomRequestsAsync()
        {
            while (!ct.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
        
                await pauseGate.WaitAsync(ct);
                await restartGate.WaitAsync(ct);
                try
                {
                    var concurrency = 10;
                    await Parallel.ForEachAsync(
                        Enumerable.Range(0, concurrency),
                        new ParallelOptions { MaxDegreeOfParallelism = concurrency, CancellationToken = ct },
                        async (_, token) =>
                        {
                            SendRandomMainApiRequestAsync(token);
                            Interlocked.Increment(ref requestCount);
                        });
                }
                finally
                {
                    pauseGate.Release();
                    restartGate.Release();
                }

                // subtract time already spent so we hit the target RPM more accurately
                var elapsed = sw.Elapsed;
                var delay = RequestInterval - elapsed;
                if (delay > TimeSpan.Zero)
                    await DelayWithJitter(delay, ct);
            }
        }

        async Task CheckForDesyncsAsync()
        {
            while (!ct.IsCancellationRequested)
            {
                await DelayWithJitter(DesyncCheckInterval, ct);

                // Acquire the gate to stop new requests, then wait 1 s before checking
                await pauseGate.WaitAsync(ct);
                try
                {
                    output.WriteLine($"[Desync check] Pausing requests for {DesyncPauseDuration.TotalSeconds}s…");
                    await Task.Delay(DesyncPauseDuration, ct);

                    var response = await fixture.SyncClient.GetAsync("api/Stats/GetDelta", ct);
                    var json     = await response.Content.ReadAsStringAsync(ct);
                    
                    var delta = JsonSerializer.Deserialize<Dictionary<string, IEnumerable<Tuple<string, Guid>>>>(json);
        
                    var count = delta.Aggregate(0, (acc, x) => acc + x.Value.Count());

                    Interlocked.Increment(ref desyncChecks);

                    if (!response.IsSuccessStatusCode || count > 0)
                    {
                        Interlocked.Add(ref desyncDocs, count);
                        Interlocked.Increment(ref desyncCount);
                        output.WriteLine($"[Desync detected] HTTP {(int)response.StatusCode}: {json}");
                    }
                    else
                    {
                        output.WriteLine($"[Desync check] OK");
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    output.WriteLine($"[Desync check error] {ex.Message}");
                }
                finally
                {
                    pauseGate.Release();
                }
            }
        }

        async Task RestartSyncServiceAsync()
        {
            while (!ct.IsCancellationRequested)
            {
                await DelayWithJitter(RestartInterval, ct);
                await restartGate.WaitAsync(ct);
                try
                {
                    output.WriteLine($"[Restart] Shutting down Sync Service for {RestartDuration.TotalSeconds} seconds");
                    await ShutdownAndRestartSyncApiAsync(ct);
                    Interlocked.Increment(ref restartCount);
                    output.WriteLine($"[Restart] Sync service restarted");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errorCount);
                    output.WriteLine($"[Restart error] {ex.Message}");
                }
                finally
                {
                    restartGate.Release();
                }
            }
        }
    }
    
    private async Task SendRandomMainApiRequestAsync(CancellationToken ct)
    {
        // Randomly pick between entity types and operations as needed
        var postEndpoints = new[]
        {
            "/api/Case/CreateRandom",
            "/api/Car/CreateRandom",
            "/api/Customer/CreateRandom",
            "/api/Employee/CreateRandom",
        };

        var patchEndpoints = new[]
        {
            "/api/Case/UpdateRandom",
            "/api/Customer/UpdateRandom",
            "/api/Employee/UpdateRandom",
            "/api/Car/UpdateRandom",
        };

        if (Random.Shared.Next(UpdatesPerCreates + 1) == 0)
        {
            var path = postEndpoints[Rng.Next(postEndpoints.Length)];
            await fixture.CaseClient.PutAsync(path, null, ct);
        }
        else
        {
            var path = patchEndpoints[Rng.Next(postEndpoints.Length)];
            await fixture.CaseClient.PatchAsync(path, null, ct);
        }
    }
    
    private async Task ShutdownAndRestartSyncApiAsync(CancellationToken ct)
    {
        await fixture.SyncClient.PostAsync("api/Test/SimulateShutdown", null, ct);
        await Task.Delay(RestartDuration, ct);
        
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

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static Task DelayWithJitter(TimeSpan baseDelay, CancellationToken ct)
    {
        var jitter   = Rng.Next(-IntervalJitterMs, IntervalJitterMs);
        var actual   = baseDelay + TimeSpan.FromMilliseconds(jitter);
        var clamped  = TimeSpan.FromMilliseconds(Math.Max(50, actual.TotalMilliseconds));
        return Task.Delay(clamped, ct);
    }
    
    private async Task<int> AmountOfDeltaDocs()
    {
        var deltaRes = await fixture.SyncClient.GetAsync("api/Stats/GetDelta");

        var json = await deltaRes.Content.ReadAsStringAsync();
        var delta = JsonSerializer.Deserialize<Dictionary<string, IEnumerable<Tuple<string, Guid>>>>(json);
        
        return delta.Aggregate(0, (acc, x) => acc + x.Value.Count());
    }
    
}