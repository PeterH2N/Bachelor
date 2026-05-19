using BsCOpenSearchSync.Business.Services;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.AspNetCore.Mvc;

namespace BsCOpenSearchSync.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SyncController(ISyncService syncService) : ControllerBase
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

    [HttpPost]
    public Task DoAll()
    {
        return syncService.DoAllSyncs();
    }
}