using BsCCaseApi.Domain.Interfaces;
using BsCOpenSearchSync.Domain.Enums;
using BsCOpenSearchSync.Domain.Models.Events;

namespace BsCOpenSearchSync.Client;

public interface ISyncEventService
{
    public Task AddSyncEvent(SyncEvent syncEvent);
    public Task<T?> DoOperation<T>(SyncType type, object obj) where T : class, IHasId;
}