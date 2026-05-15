using BsCOpenSearchSync.Domain.Models.Events;

namespace BsCOpenSearchSync.Business.Services;

public interface ISyncService
{
    public Task<SyncEvent> GetEventById(int id);
    public Task<List<SyncEvent>> GetAllEvents();
    public Task<string> DoSync(int eventId);
    public Task DoAllSyncs();
}