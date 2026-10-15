using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/demo")]
public class DemoController : ControllerBase
{
    private readonly IDemoManager _demoManager;

    public DemoController(IDemoManager demoManager)
    {
        _demoManager = demoManager;
    }

    public record LoadDemoRequest(string? CenterId = null, bool OverrideIfRealExists = false);

    [HttpGet("status")]
    [HttpGet("status/{centerId}")]
    public ActionResult GetStatus(
        [FromRoute] string? centerId = null,
        [FromQuery] string? queryCenterId = null,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null)
    {
        string targetCenter = centerId ?? queryCenterId ?? headerCenterId ?? "ctr-madrid-1";
        var (hasDemo, demoCount, realCount) = _demoManager.GetStatus(targetCenter);
        return Ok(new
        {
            hasDemoData = hasDemo,
            demoCount,
            realCount,
            demoEscandallosCount = demoCount,
            realEscandallosCount = realCount
        });
    }

    [HttpPost("load")]
    [HttpPost("load/{centerId}")]
    public ActionResult LoadDemoData(
        [FromRoute] string? centerId = null,
        [FromQuery] bool? overrideIfRealExists = null,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromBody] LoadDemoRequest? body = null)
    {
        string targetCenter = centerId ?? body?.CenterId ?? headerCenterId ?? "ctr-madrid-1";
        bool doOverride = overrideIfRealExists ?? body?.OverrideIfRealExists ?? false;
        _demoManager.LoadDemoData(targetCenter, doOverride);
        return Ok(new { success = true, centerId = targetCenter });
    }

    [HttpPost("purge")]
    [HttpPost("purge/{centerId}")]
    public ActionResult PurgeDemoData(
        [FromRoute] string? centerId = null,
        [FromQuery] string? queryCenterId = null,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromBody] LoadDemoRequest? body = null)
    {
        string targetCenter = centerId ?? queryCenterId ?? body?.CenterId ?? headerCenterId ?? "ctr-madrid-1";
        int deleted = _demoManager.PurgeDemoData(targetCenter);
        return Ok(new
        {
            purgedCount = deleted,
            purgedRecordsCount = deleted,
            centerId = targetCenter
        });
    }
}
