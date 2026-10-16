using Escandallo.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/system")]
public class SystemController : ControllerBase
{
    private readonly ISystemLogBuffer _logBuffer;

    public SystemController(ISystemLogBuffer logBuffer)
    {
        _logBuffer = logBuffer;
    }

    [HttpGet("logs")]
    public ActionResult<IEnumerable<LogMessageEntry>> GetLogs([FromQuery] int limit = 100)
    {
        var logs = _logBuffer.GetRecentLogs(limit);
        return Ok(logs);
    }
}
