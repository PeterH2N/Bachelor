using BsCOpenSearchSync.DataAccess.Store;
using BsCOpenSearchSync.Domain.Models.Events;

namespace BsCOpenSearchSync.Client;

public class SyncEventService(EventDbContext dbContext) : ISyncEventService
{
    public async Task AddSyncEvent(SyncEvent syncEvent)
    {
        await dbContext.SyncEvents.AddAsync(syncEvent);
        await dbContext.SaveChangesAsync();
    }
}