using BsCOpenSearchSync.DataAccess.Store;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.EntityFrameworkCore;

namespace BsCOpenSearchSync.Business.Services;

public class SyncService(EventDbContext eventDbContext) : ISyncService
{
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
}