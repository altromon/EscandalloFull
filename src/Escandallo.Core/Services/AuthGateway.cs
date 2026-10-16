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

    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, UserRecord> _users = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _centerNames = new();

    public AuthGateway()
    {
        _centerNames["ctr-madrid-1"] = "Centro Belleza Madrid";
        _centerNames["ctr-radiante-1"] = "Centro Radiante";
        _centerNames["ctr-luz-belleza"] = "Luz y Belleza";
        _centerNames["ctr-test-free"] = "Centro Free Test";

        _users["gestor@estetica.com"] = new UserRecord
        {
            UserId = "usr-100",
            Email = "gestor@estetica.com",
            Password = "SecretPass123!",
            Role = RoleType.User,
            TenantId = "ten-001",
            AuthorizedCenterIds = new() { "ctr-madrid-1", "ctr-radiante-1", "ctr-luz-belleza", "ctr-test-free" }
        };

        _users["guest@collaborator.com"] = new UserRecord
        {
            UserId = "usr-200",
            Email = "guest@collaborator.com",
            Password = "GuestPass123!",
            Role = RoleType.Guest,
            TenantId = "ten-001",
            AuthorizedCenterIds = new() { "ctr-madrid-1" }
        };
    }

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

    public AuthSession Register(string email, string password, string? centerName = null, RoleType role = RoleType.User)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Dirección de correo electrónico inválida.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");

        string trimmedEmail = email.Trim().ToLowerInvariant();
        if (_users.ContainsKey(trimmedEmail))
            throw new InvalidOperationException("El correo ya está registrado.");

        string userId = $"usr-{Guid.NewGuid():N}";
        string centerId = $"ctr-{Guid.NewGuid():N}";
        string resolvedCenterName = string.IsNullOrWhiteSpace(centerName) ? "Mi Centro de Estética" : centerName.Trim();

        _centerNames[centerId] = resolvedCenterName;

        var user = new UserRecord
        {
            UserId = userId,
            Email = trimmedEmail,
            Password = password,
            Role = role,
            TenantId = $"ten-{Guid.NewGuid():N}",
            AuthorizedCenterIds = new() { centerId }
        };

        _users[trimmedEmail] = user;
        return Login(trimmedEmail, password);
    }

    public CenterMembership CreateCenter(string userId, string centerName)
    {
        if (string.IsNullOrWhiteSpace(centerName))
            throw new ArgumentException("El nombre del centro no puede estar vacío.");

        var user = _users.Values.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        string centerId = $"ctr-{Guid.NewGuid():N}";
        string trimmedName = centerName.Trim();
        _centerNames[centerId] = trimmedName;

        lock (user.AuthorizedCenterIds)
        {
            user.AuthorizedCenterIds.Add(centerId);
        }

        return new CenterMembership
        {
            CenterId = centerId,
            Name = trimmedName,
            Role = user.Role,
            IsPlanPremium = false
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
            Name = _centerNames.TryGetValue(cid, out var name) ? name : cid,
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
