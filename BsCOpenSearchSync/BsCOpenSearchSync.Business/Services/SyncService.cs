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

    public async Task<string> DoSync(int eventId)
    {
        var syncEvent = await eventDbContext.SyncEvents.FindAsync(eventId);

        if (syncEvent is null)
        {
            throw new Exception("Sync event not found");
        }
        if (syncEvent.Status is not SyncStatus.Waiting)
        {
            throw new Exception($"Sync status is {nameof(SyncStatus.Waiting)}");
        }
        
        return _jsonProcessor.JsonBulkFromId(syncEvent.Type, syncEvent.ObjectId, syncEvent.TableName);
    }

    public async Task DoAllSyncs()
    {
        // wait for other threads to process the events
        await WaitForEventsAsync();
        
        // Atomically claim the events
        await eventDbContext.SyncEvents
            .Where(se => se.Status == SyncStatus.Waiting)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, SyncStatus.Syncing));

        // Load only the ones we just claimed
        var eventList = await eventDbContext.SyncEvents
            .Where(se => se.Status == SyncStatus.Syncing)
            .OrderBy(e => e.Id)
            .ToListAsync();

        if (eventList.Count == 0)
        {
            logger.LogInformation("No events found");
            return;
        }
        
        var bulkJson = _jsonProcessor.BulkJsonFromIds(eventList);
        
        // post to opensearch
        var bulkResponse = await openSearchClient.BulkAsync<StringResponse>(PostData.String(bulkJson));
        
        if (!bulkResponse.Success)
        {
            logger.LogError(bulkResponse.OriginalException, "Bulk request failed");
            throw new Exception("Bulk response failed");
        }

        // Also check for partial failures in the body
        logger.LogDebug("Bulk response: {Body}", bulkResponse.Body);
        
        // update event status
        await eventDbContext.SyncEvents
            .Where(se => se.Status == SyncStatus.Syncing)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, SyncStatus.Processed));
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

            await Task.Delay(interval, cancellationToken);
        }
    }
}