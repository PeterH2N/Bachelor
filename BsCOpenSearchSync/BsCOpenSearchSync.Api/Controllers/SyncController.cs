using BsCOpenSearchSync.Business.Services;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.AspNetCore.Mvc;

namespace BsCOpenSearchSync.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SyncController(ISyncService syncService)
{
    [HttpGet("{id:int}")]
    public Task<SyncEvent> GetEvent(int id)
    {
        return syncService.GetEventById(id);
    }
    
    [HttpGet]
    public Task<List<SyncEvent>> GetAllEvents()
    {
        return syncService.GetAllEvents();
    }

    [HttpPost("{eventId:int}")]
    public Task<string> DoSync(int eventId)
    {
        return syncService.DoSync(eventId);
    }

    [HttpPost]
    public Task DoAll()
    {
        return syncService.DoAllSyncs();
    }
}