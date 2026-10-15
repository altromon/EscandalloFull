using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    private readonly IHealthCheckService _healthCheck;

    public HealthController(IHealthCheckService healthCheck)
    {
        _healthCheck = healthCheck;
    }

    [HttpGet("/healthz")]
    public IActionResult GetLiveness()
    {
        var (status, responseTimeMs) = _healthCheck.CheckLiveness();
        return Ok(new { status, responseTimeMs, timestamp = DateTime.UtcNow });
    }

    [HttpGet("/readyz")]
    public IActionResult GetReadiness()
    {
        var (status, componentDetails) = _healthCheck.CheckReadiness();
        return Ok(new { status, componentDetails, timestamp = DateTime.UtcNow });
    }
}
