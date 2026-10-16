using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditTrailRepository _auditTrailRepo;

    public AuditController(IAuditTrailRepository auditTrailRepo)
    {
        _auditTrailRepo = auditTrailRepo;
    }

    [HttpGet]
    [HttpGet("history")]
    public ActionResult<IEnumerable<AuditEntry>> GetHistory(
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null,
        [FromQuery] string? resourceId = null)
    {
        string targetCenter = !string.IsNullOrWhiteSpace(centerId) ? centerId : (!string.IsNullOrWhiteSpace(headerCenterId) ? headerCenterId : "ctr-madrid-1");
        var entries = _auditTrailRepo.GetHistory(targetCenter, resourceId);
        return Ok(entries);
    }
}
