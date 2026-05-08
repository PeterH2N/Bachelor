using BsCOpenSearchSync.Domain.Models.Events;

namespace BsCOpenSearchSync.DataAccess.Services;

public interface ISyncEventService
{
    public Task AddSyncEvent(SyncEvent syncEvent);
}