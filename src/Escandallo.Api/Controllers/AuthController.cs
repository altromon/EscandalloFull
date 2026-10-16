using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthGateway _authGateway;
    private readonly IAuditTrailRepository? _auditTrailRepo;

    public AuthController(IAuthGateway authGateway, IAuditTrailRepository? auditTrailRepo = null)
    {
        _authGateway = authGateway;
        _auditTrailRepo = auditTrailRepo;
    }

    public record LoginDto(string Email, string Password);
    public record SelectCenterDto(string UserId, string CenterId);
    public record RegisterDto(string Email, string Password, string? CenterName = null);
    public record CreateCenterDto(string? UserId, string CenterName);

    [HttpPost("register")]
    public ActionResult<AuthSession> Register([FromBody] RegisterDto request)
    {
        try
        {
            var session = _authGateway.Register(request.Email, request.Password, request.CenterName);
            return Ok(session);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public ActionResult<AuthSession> Login([FromBody] LoginDto request)
    {
        try
        {
            var session = _authGateway.Login(request.Email, request.Password);
            return Ok(session);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("centers/{userId}")]
    public ActionResult<IEnumerable<CenterMembership>> GetAuthorizedCenters(string userId)
    {
        var centers = _authGateway.GetAuthorizedCenters(userId);
        return Ok(centers);
    }

    [HttpPost("centers")]
    public ActionResult<CenterMembership> CreateCenter(
        [FromBody] CreateCenterDto request,
        [FromHeader(Name = "X-User-Id")] string? headerUserId = null)
    {
        string? targetUserId = request.UserId ?? headerUserId;
        if (string.IsNullOrWhiteSpace(targetUserId))
        {
            return BadRequest(new { message = "Se requiere el ID de usuario." });
        }

        try
        {
            var membership = _authGateway.CreateCenter(targetUserId, request.CenterName);
            _auditTrailRepo?.RecordEntry(new AuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                CenterId = membership.CenterId,
                ActorId = targetUserId,
                TimestampUtc = DateTime.UtcNow,
                PreviousValue = 0,
                NewValue = 1,
                ChangeType = $"CREATE_CENTER:{membership.Name}"
            });
            return Ok(membership);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("select-center")]
    public ActionResult<AuthSession> SelectCenter([FromBody] SelectCenterDto request)
    {
        try
        {
            var session = _authGateway.SelectCenter(request.UserId, request.CenterId);
            return Ok(session);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }
}
