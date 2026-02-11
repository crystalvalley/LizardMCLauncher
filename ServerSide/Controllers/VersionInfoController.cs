using Microsoft.AspNetCore.Mvc;
using ServerSide.Services;

namespace ServerSide.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VersionInfoController(VersionInfoService _versionInfoService) : ControllerBase
{
    [HttpGet]
    public ActionResult<string> GetLatestVersionInfo()
    {
        var versionInfo = _versionInfoService.GetLastVersionInfo();
        return Ok(versionInfo);
    }
}
