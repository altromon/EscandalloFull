using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/demo")]
public class DemoController : ControllerBase
{
    private readonly IDemoManager _demoManager;
    private readonly IAuditTrailRepository? _auditTrailRepo;

    public DemoController(IDemoManager demoManager, IAuditTrailRepository? auditTrailRepo = null)
    {
        _demoManager = demoManager;
        _auditTrailRepo = auditTrailRepo;
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
        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = 0,
            NewValue = 3,
            ChangeType = "LOAD_DEMO_DATA:Carga de datos demostrativos oficiales"
        });
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
        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = deleted,
            NewValue = 0,
            ChangeType = $"PURGE_DEMO_DATA:Depuración de {deleted} registros demo"
        });
        return Ok(new
        {
            purgedCount = deleted,
            purgedRecordsCount = deleted,
            centerId = targetCenter
        });
    }
}
