using Microsoft.AspNetCore.Mvc;

namespace ServerSide.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ResourceController : ControllerBase
{
    [HttpGet("launcher")]
    public async Task<ActionResult<string>> GetLauncherResourceUrl([FromServices] Services.CloudRestService cloudRestService)
    {
        var url = await cloudRestService.GetLauncherDownloadUrl();
        return Ok(url);
    }

    [HttpGet("neoforge")]
    public ActionResult<string> GetNeoforgeResourceUrl([FromServices] Services.CloudRestService cloudRestService, [FromQuery] string neoforgeVersion)
    {
        var url = cloudRestService.GetNeoforgeDownloadUrl(neoforgeVersion).GetAwaiter().GetResult();
        return Ok(url);
    }
}
