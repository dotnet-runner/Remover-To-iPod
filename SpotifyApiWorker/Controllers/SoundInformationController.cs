using Microsoft.AspNetCore.Mvc;
using SpotifyApiWorker.Services.Implementations;

namespace SpotifyApiWorker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SoundInformationController: ControllerBase
{
    public SoundInformationController(RedisService redis)
    {
    }
    
    [HttpGet("playlist")]
    public IActionResult GetPlaylists()
    {
        return Ok();
    }
}