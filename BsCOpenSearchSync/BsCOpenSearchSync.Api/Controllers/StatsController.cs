using BsCOpenSearchSync.Business.Services;
using Microsoft.AspNetCore.Mvc;
using OpenSearch.Net;

namespace BsCOpenSearchSync.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class StatsController(IStatsService statsService) : ControllerBase
{
    [HttpGet("{index}")]
    public Task<string> GetCount(string index)
    {
        return statsService.GetCount(index);
    }
    
    [HttpGet]
    public Task<Dictionary<Type, IEnumerable<Guid>>> GetDelta()
    {
        return statsService.GetDelta();
    }
}