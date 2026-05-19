using BsCOpenSearchSync.Models;
using Microsoft.AspNetCore.Mvc;

namespace BsCOpenSearchSync.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class TestController(IWebHostEnvironment env) : ControllerBase
{
    [HttpPost]
    public IActionResult SimulateUnresponsive([FromQuery] int seconds = 30)
    {
        if (!env.IsDevelopment())
        {
            return BadRequest();
        }
        SyncServiceFlags.IsUnresponsive = true;
        SyncServiceFlags.UnresponsiveDuration = TimeSpan.FromSeconds(seconds);
        
        return Ok();
    }
    
    [HttpPost]
    public IActionResult SimulateShutdown(IHostApplicationLifetime lifetime, [FromQuery] int delaySeconds = 0)
    {
        if (!env.IsDevelopment())
            return NotFound();

        _ = Task.Delay(TimeSpan.FromSeconds(delaySeconds)).ContinueWith(_ =>
        {
            lifetime.StopApplication();
        });

        return Ok();
    }

    [HttpPost]
    public IActionResult Reset()
    {
        if (!env.IsDevelopment())
        {
            return BadRequest();
        }
        SyncServiceFlags.IsUnresponsive = false;
        return Ok();
    }
}