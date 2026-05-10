using BsCOpenSearchSync.Domain.Models.Events;

namespace BsCOpenSearchSync.Client;

public interface ISyncEventService
{
    public Task AddSyncEvent(SyncEvent syncEvent);
}