using BsCOpenSearchSync.Business.Helpers;
using BsCOpenSearchSync.DataAccess.Store;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.Business.Services;

public class SyncService(EventDbContext eventDbContext, DbContext dbContext) : ISyncService
{
    private JsonProcessor _jsonProcessor = new JsonProcessor(dbContext);
    
    public async Task<LatestSync> GetLatestSync()
    {
        return await eventDbContext.LatestSyncs.FindAsync(1) ?? throw new Exception("No sync found");
    }

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
        
        return _jsonProcessor.JsonBulkFromId(syncEvent.ObjectId, syncEvent.TableName);
    }
}