using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthGateway _authGateway;

    public AuthController(IAuthGateway authGateway)
    {
        _authGateway = authGateway;
    }

    public record LoginDto(string Email, string Password);
    public record SelectCenterDto(string UserId, string CenterId);

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
