using BsCOpenSearchSync.Business.Helpers;
using BsCOpenSearchSync.DataAccess.Store;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenSearch.Net;

namespace BsCOpenSearchSync.Business.Services;

public class SyncService(EventDbContext eventDbContext, DbContext dbContext, IOpenSearchLowLevelClient openSearchClient, ILogger<SyncService> logger) : ISyncService
{
    private readonly JsonProcessor _jsonProcessor = new JsonProcessor(dbContext);

    public async Task<SyncEvent> GetEventById(int id)
    {
        return await eventDbContext.SyncEvents.FindAsync(id) ?? throw new Exception("Event not found");
    }

    public async Task<List<SyncEvent>> GetAllEvents()
    {
        return await eventDbContext.SyncEvents.ToListAsync();
    }

    public async Task DoAllSyncs()
    {
        // wrap everything in a try catch, so if we ever fail, events are flagged as waiting
        try
        {
            // early exit if no events are waiting
            if (!await eventDbContext.SyncEvents.AnyAsync(se => se.Status == SyncStatus.Waiting))
            {
                return;
            }
            // wait for other threads to process the events
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await WaitForEventsAsync(cancellationToken: timeoutCts.Token);
        
            // Atomically claim the events
            await eventDbContext.SyncEvents
                .Where(se => se.Status == SyncStatus.Waiting)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, SyncStatus.Syncing));

            // Load only the ones we just claimed
            var eventList = await eventDbContext.SyncEvents
                .Where(se => se.Status == SyncStatus.Syncing)
                .OrderBy(e => e.Id)
                .ToListAsync();
        
            // early exit if we claimed no events
            if (eventList.Count == 0)
            {
                return;
            }
        
            var bulkJson = _jsonProcessor.BulkJsonFromIds(eventList);
        
            // post to opensearch
            var bulkResponse = await openSearchClient.BulkAsync<StringResponse>(PostData.String(bulkJson));
        
            if (!bulkResponse.Success)
            {
                throw new Exception("Bulk operation failed");
            }
            // update event status
            await eventDbContext.SyncEvents
                .Where(se => se.Status == SyncStatus.Syncing)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, SyncStatus.Processed));
        }
        catch 
        {
            // set events status to waiting
            await eventDbContext.SyncEvents
                .Where(se => se.Status == SyncStatus.Syncing)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, SyncStatus.Waiting));
            throw;
        }
    }
    
    private async Task WaitForEventsAsync(
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default)
    {
        var interval = pollInterval ?? TimeSpan.FromSeconds(0.5);

        while (true)
        {
            var hasProcessing = await eventDbContext.Set<SyncEvent>()
                .AnyAsync(e => e.Status == SyncStatus.Syncing, cancellationToken);

            if (!hasProcessing)
                return;

            try
            {
                await Task.Delay(interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return; // timed out, return gracefully
            }
        }
    }
}