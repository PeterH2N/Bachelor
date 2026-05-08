using BsCCaseApi.Domain.Models;
using BsCOpenSearchSync.Business.Services;
using BsCOpenSearchSync.Domain.Models.Events;
using Microsoft.AspNetCore.Mvc;

namespace BsCOpenSearchSync.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SyncController(ISyncService syncService)
{
    [HttpGet]
    public Task<LatestSync> GetLatestSync()
    {
        return syncService.GetLatestSync();
    }
    
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
}