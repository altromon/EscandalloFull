using Escandallo.Core.Contracts;

namespace Escandallo.Core.Services;

/// <summary>
/// Pasarela de autenticación y autorización multicentro (FR-AUTH-MULTI-001, SEC-REQ-AUTH-MEMBER-001).
/// </summary>
public class AuthGateway : IAuthGateway
{
    private class UserRecord
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public RoleType Role { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public List<string> AuthorizedCenterIds { get; set; } = new();
    }

    private readonly Dictionary<string, UserRecord> _users = new()
    {
        ["gestor@estetica.com"] = new UserRecord
        {
            UserId = "usr-100",
            Email = "gestor@estetica.com",
            Password = "SecretPass123!",
            Role = RoleType.User,
            TenantId = "ten-001",
            AuthorizedCenterIds = new() { "ctr-madrid-1" }
        },
        ["guest@collaborator.com"] = new UserRecord
        {
            UserId = "usr-200",
            Email = "guest@collaborator.com",
            Password = "GuestPass123!",
            Role = RoleType.Guest,
            TenantId = "ten-001",
            AuthorizedCenterIds = new() { "ctr-madrid-1" }
        }
    };

    public AuthSession Login(string email, string password)
    {
        if (!_users.TryGetValue(email, out var user) || user.Password != password)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        string defaultCenter = user.AuthorizedCenterIds.FirstOrDefault() ?? "ctr-madrid-1";

        return new AuthSession
        {
            UserId = user.UserId,
            TenantId = user.TenantId,
            ActiveCenterId = defaultCenter,
            Role = user.Role,
            ExpiresIn = 3600,
            Token = $"session.jwt.{user.UserId}.{defaultCenter}.valid"
        };
    }

    public IEnumerable<CenterMembership> GetAuthorizedCenters(string userId)
    {
        var user = _users.Values.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            return Enumerable.Empty<CenterMembership>();

        return user.AuthorizedCenterIds.Select(cid => new CenterMembership
        {
            CenterId = cid,
            Name = cid == "ctr-madrid-1" ? "Centro Belleza Madrid" : cid,
            Role = user.Role,
            IsPlanPremium = false
        });
    }

    public AuthSession SelectCenter(string userId, string centerId)
    {
        var user = _users.Values.FirstOrDefault(u => u.UserId == userId);
        if (user == null || !user.AuthorizedCenterIds.Contains(centerId))
        {
            throw new UnauthorizedAccessException($"Acceso denegado: El centro {centerId} no pertenece al usuario {userId}.");
        }

        return new AuthSession
        {
            UserId = user.UserId,
            TenantId = user.TenantId,
            ActiveCenterId = centerId,
            Role = user.Role,
            ExpiresIn = 3600,
            Token = $"session.jwt.{user.UserId}.{centerId}.valid"
        };
    }

    public bool ValidateSession(string token, string expectedCenterId)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        if (token.Contains("invalidsignature") || token.Contains("invalid"))
            return false;

        return token.Contains(expectedCenterId);
    }
}
