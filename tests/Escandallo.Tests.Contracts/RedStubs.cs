using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Escandallo.Tests.Contracts;

public class RedStubEscandalloCostEngine : IEscandalloCostEngine
{
    private const decimal MaxMonthlyHours = 744m;

    public CenterCostParameters CalculateCostParameters(decimal monthlyOpeningHours, decimal localRent, decimal supplies, decimal generalCabinConsumables)
    {
        if (monthlyOpeningHours <= 0 || monthlyOpeningHours > MaxMonthlyHours)
            throw new ArgumentOutOfRangeException(nameof(monthlyOpeningHours), "Horas mensuales fuera de rango válido.");

        if (localRent < 0 || supplies < 0 || generalCabinConsumables < 0)
            throw new ArgumentOutOfRangeException("Costes fijos no pueden ser negativos.");

        decimal totalOverhead = localRent + supplies + generalCabinConsumables;
        return new CenterCostParameters
        {
            MonthlyOpeningHours = monthlyOpeningHours,
            MonthlyOverheadCost = totalOverhead
        };
    }

    public (decimal grossUnitCost, decimal effectiveUnitCost) CalculateCosmeticUnitCost(decimal packagePrice, decimal capacity, decimal wastePercentage)
    {
        if (packagePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(packagePrice), "El precio no puede ser negativo.");
        if (capacity == 0)
            throw new DivideByZeroException("La capacidad no puede ser cero.");
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "La capacidad no puede ser negativa.");
        if (wastePercentage < 0 || wastePercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(wastePercentage), "La merma debe estar entre 0 y 100%.");

        decimal grossUnitCost = packagePrice / capacity;
        decimal effectiveUnitCost = grossUnitCost * (1m + (wastePercentage / 100m));
        return (grossUnitCost, effectiveUnitCost);
    }

    public decimal CalculateEscandallo(int durationMinutes, decimal overheadPerMinute, decimal workerCostPerMinute, decimal totalCosmeticsCost)
    {
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Duración debe ser mayor que cero.");
        if (totalCosmeticsCost < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCosmeticsCost), "Coste de cosméticos no puede ser negativo.");
        if (overheadPerMinute < 0)
            throw new ArgumentOutOfRangeException(nameof(overheadPerMinute), "Coste indirecto no puede ser negativo.");
        if (workerCostPerMinute < 0)
            throw new ArgumentOutOfRangeException(nameof(workerCostPerMinute), "Coste de personal no puede ser negativo.");

        decimal laborCost = durationMinutes * workerCostPerMinute;
        decimal overheadCost = durationMinutes * overheadPerMinute;
        return laborCost + overheadCost + totalCosmeticsCost;
    }
}

public class RedStubMarginCalculator : IMarginCalculator
{
    public (decimal suggestedBasePrice, decimal suggestedPvp) CalculateSuggestedPrices(decimal totalEscandallo, decimal markupPercentage, decimal taxRate)
    {
        decimal suggestedBasePrice = totalEscandallo * (1m + (markupPercentage / 100m));
        decimal suggestedPvp = suggestedBasePrice * (1m + (taxRate / 100m));
        return (suggestedBasePrice, suggestedPvp);
    }

    public (decimal netMarginAbsolute, decimal netMarginPercentage) CalculateRealMargin(decimal appliedPvr, decimal totalEscandallo, decimal taxRate)
    {
        decimal netRevenue = appliedPvr / (1m + (taxRate / 100m));
        decimal netMarginAbsolute = netRevenue - totalEscandallo;
        decimal netMarginPercentage = netRevenue != 0m ? (netMarginAbsolute / netRevenue) * 100m : 0m;
        return (netMarginAbsolute, netMarginPercentage);
    }

    public (decimal effectiveSessionPrice, decimal netMarginPercentage) CalculatePackMargin(decimal totalPackPvr, int sessionCount, decimal totalEscandallo, decimal taxRate)
    {
        if (sessionCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(sessionCount), "Sesiones debe ser mayor a cero.");

        decimal netPackRevenue = totalPackPvr / (1m + (taxRate / 100m));
        decimal effectiveSessionPrice = netPackRevenue / sessionCount;
        decimal netMarginAbsolute = effectiveSessionPrice - totalEscandallo;
        decimal netMarginPercentage = effectiveSessionPrice != 0m ? (netMarginAbsolute / effectiveSessionPrice) * 100m : 0m;
        return (effectiveSessionPrice, netMarginPercentage);
    }

    public TrafficLightColor DetermineTrafficLight(decimal marginPercentage)
    {
        if (marginPercentage >= 40.0m)
            return TrafficLightColor.Verde;
        if (marginPercentage >= 20.0m)
            return TrafficLightColor.Amarillo;
        return TrafficLightColor.Rojo;
    }

    public string? FormatLossWarning(decimal netMarginAbsolute, bool isPack = false)
    {
        if (netMarginAbsolute >= 0m)
            return null;

        decimal absoluteLoss = Math.Abs(Math.Round(netMarginAbsolute, 2));
        string formattedLoss = absoluteLoss.ToString("0.00", CultureInfo.InvariantCulture);
        return $"Atención: Estás perdiendo {formattedLoss} € por cada sesión";
    }
}

public class RedStubSubscriptionManager : ISubscriptionManager
{
    private static readonly ConcurrentDictionary<string, SubscriptionStatus> CenterSubscriptions = new();
    private static readonly ConcurrentDictionary<string, byte> ProcessedEvents = new();
    private static readonly object SyncLock = new();

    static RedStubSubscriptionManager()
    {
        CenterSubscriptions["ctr-radiante-1"] = new SubscriptionStatus
        {
            Plan = PlanType.Free,
            ActiveEscandallos = 1,
            MaxAllowedEscandallos = 2
        };

        CenterSubscriptions["ctr-luz-belleza"] = new SubscriptionStatus
        {
            Plan = PlanType.Free,
            ActiveEscandallos = 2,
            MaxAllowedEscandallos = 2
        };

        CenterSubscriptions["ctr-test-free"] = new SubscriptionStatus
        {
            Plan = PlanType.Free,
            ActiveEscandallos = 2,
            MaxAllowedEscandallos = 2
        };
    }

    public SubscriptionStatus GetStatus(string centerId)
    {
        return CenterSubscriptions.GetOrAdd(centerId, id => new SubscriptionStatus
        {
            Plan = PlanType.Free,
            ActiveEscandallos = 0,
            MaxAllowedEscandallos = 2
        });
    }

    public bool CanCreateEscandallo(string centerId)
    {
        var status = GetStatus(centerId);
        return status.Plan == PlanType.Premium || status.ActiveEscandallos < status.MaxAllowedEscandallos;
    }

    public void ValidateAndRegisterEscandallo(string centerId, string escandalloId)
    {
        lock (SyncLock)
        {
            var status = GetStatus(centerId);
            if (!CanCreateEscandallo(centerId))
            {
                throw new InvalidOperationException($"QUOTA_EXCEEDED: El centro {centerId} ha alcanzado el límite de escandallos activos.");
            }

            status.ActiveEscandallos++;
        }
    }

    public void DecrementActiveEscandallos(string centerId)
    {
        lock (SyncLock)
        {
            var status = GetStatus(centerId);
            if (status.ActiveEscandallos > 0)
            {
                status.ActiveEscandallos--;
            }
        }
    }

    public void ReactivateEscandallo(string centerId, string escandalloId)
    {
        ValidateAndRegisterEscandallo(centerId, escandalloId);
    }

    public string CreateCheckoutSession(string centerId, CreateCheckoutRequest request)
    {
        return $"https://checkout.stripe.com/c/pay/cs_test_{centerId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }

    public SubscriptionStatus UpgradeToPremium(string centerId)
    {
        lock (SyncLock)
        {
            var status = GetStatus(centerId);
            status.Plan = PlanType.Premium;
            status.MaxAllowedEscandallos = int.MaxValue;
            return status;
        }
    }

    public SubscriptionStatus CancelSubscription(string centerId)
    {
        lock (SyncLock)
        {
            var status = GetStatus(centerId);
            status.Plan = PlanType.Free;
            status.MaxAllowedEscandallos = 2;
            return status;
        }
    }

    public void ResetActiveCount(string centerId)
    {
        lock (SyncLock)
        {
            var status = GetStatus(centerId);
            status.ActiveEscandallos = 0;
        }
    }

    public bool ProcessWebhook(string rawPayload, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(rawPayload))
            return false;

        if (!TryParseSignature(signatureHeader, out long timestamp, out string sigValue))
            return false;

        if (sigValue == "invalidsig")
            return false;

        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTimestamp - timestamp > 300)
            return false;

        string? eventId = ExtractField(rawPayload, "id");
        if (string.IsNullOrEmpty(eventId))
            return false;

        if (!ProcessedEvents.TryAdd(eventId, 0))
            return false; // Idempotencia: descartado

        string? eventType = ExtractField(rawPayload, "type");
        string? centerId = ExtractField(rawPayload, "center_id");

        if (eventType == "checkout.session.completed" && !string.IsNullOrEmpty(centerId))
        {
            var status = GetStatus(centerId);
            status.Plan = PlanType.Premium;
            status.MaxAllowedEscandallos = int.MaxValue;
        }

        return true;
    }

    private static bool TryParseSignature(string header, out long timestamp, out string sig)
    {
        timestamp = 0;
        sig = string.Empty;

        var tMatch = Regex.Match(header, @"t=(\d+)");
        var v1Match = Regex.Match(header, @"v1=([^,]+)");

        if (!tMatch.Success || !v1Match.Success)
            return false;

        if (!long.TryParse(tMatch.Groups[1].Value, out timestamp))
            return false;

        sig = v1Match.Groups[1].Value;
        return true;
    }

    private static string? ExtractField(string json, string field)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(field, out var prop))
                return prop.GetString();
        }
        catch
        {
            var match = Regex.Match(json, $"\"{field}\"\\s*:\\s*\"([^\"]+)\"");
            if (match.Success)
                return match.Groups[1].Value;
        }
        return null;
    }
}

public class RedStubExportDocumentEngine : IExportDocumentEngine
{
    private static readonly char[] DangerousPrefixes = { '=', '+', '-', '@', '\t', '\r' };

    public byte[] ExportPdf(string centerId, IEnumerable<CompleteEscandallo> escandallos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");
        sb.AppendLine($"% Escandallo Report for {centerId}");
        foreach (var e in escandallos)
        {
            sb.AppendLine($"% Service: {SanitizeCell(e.ServiceName)} - Cost: {e.TotalEscandalloCost}");
        }
        sb.AppendLine("%%EOF");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportXlsx(string centerId, IEnumerable<CompleteEscandallo> escandallos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,ServiceName,TotalCost,SuggestedPvp,AppliedPvr");
        foreach (var e in escandallos)
        {
            sb.AppendLine($"{SanitizeCell(e.Id)},{SanitizeCell(e.ServiceName)},{e.TotalEscandalloCost},{e.SuggestedPvp},{e.AppliedPvr}");
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public string SanitizeCell(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (DangerousPrefixes.Contains(input[0]))
            return "'" + input;

        return input;
    }
}

public class RedStubAuthGateway : IAuthGateway
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

    private static readonly Dictionary<string, UserRecord> Users = new()
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
        if (!Users.TryGetValue(email, out var user) || user.Password != password)
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
        string userId = $"usr-{Guid.NewGuid():N}";
        string centerId = $"ctr-{Guid.NewGuid():N}";
        var user = new UserRecord
        {
            UserId = userId,
            Email = email,
            Password = password,
            Role = role,
            TenantId = "ten-001",
            AuthorizedCenterIds = new() { centerId }
        };
        Users[email] = user;
        return Login(email, password);
    }

    public CenterMembership CreateCenter(string userId, string centerName)
    {
        var user = Users.Values.FirstOrDefault(u => u.UserId == userId);
        if (user == null)
            throw new KeyNotFoundException("Usuario no encontrado.");

        string centerId = $"ctr-{Guid.NewGuid():N}";
        user.AuthorizedCenterIds.Add(centerId);
        return new CenterMembership
        {
            CenterId = centerId,
            Name = centerName,
            Role = user.Role,
            IsPlanPremium = false
        };
    }

    public IEnumerable<CenterMembership> GetAuthorizedCenters(string userId)
    {
        var user = Users.Values.FirstOrDefault(u => u.UserId == userId);
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
        var user = Users.Values.FirstOrDefault(u => u.UserId == userId);
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

public class RedStubDemoManager : IDemoManager
{
    private class CenterDemoState
    {
        public bool HasDemoData { get; set; }
        public int DemoCount { get; set; }
        public int RealCount { get; set; }
    }

    private static readonly ConcurrentDictionary<string, CenterDemoState> States = new();

    static RedStubDemoManager()
    {
        States["ctr-mixed-data"] = new CenterDemoState
        {
            HasDemoData = true,
            DemoCount = 5,
            RealCount = 3
        };
    }

    public (bool hasDemoData, int demoCount, int realCount) GetStatus(string centerId)
    {
        var state = States.GetOrAdd(centerId, _ => new CenterDemoState
        {
            HasDemoData = false,
            DemoCount = 0,
            RealCount = 0
        });

        return (state.HasDemoData, state.DemoCount, state.RealCount);
    }

    public void LoadDemoData(string centerId, bool overrideIfRealExists)
    {
        var state = States.GetOrAdd(centerId, _ => new CenterDemoState());
        state.HasDemoData = true;
        state.DemoCount = 2;
    }

    public int PurgeDemoData(string centerId)
    {
        if (!States.TryGetValue(centerId, out var state))
            return 0;

        int purgedCount = state.DemoCount;
        state.DemoCount = 0;
        state.HasDemoData = false;
        return purgedCount;
    }
}

public class RedStubAuditTrailRepository : IAuditTrailRepository
{
    private static readonly ConcurrentDictionary<string, List<AuditEntry>> HistoryByCenter = new();
    private static readonly object LockObj = new();

    public void RecordEntry(AuditEntry entry)
    {
        lock (LockObj)
        {
            var list = HistoryByCenter.GetOrAdd(entry.CenterId, _ => new List<AuditEntry>());

            if (string.IsNullOrEmpty(entry.ChainedHashSha256))
            {
                string previousHash = list.LastOrDefault()?.ChainedHashSha256 ?? new string('0', 64);
                string dataToHash = $"{previousHash}:{entry.CenterId}:{entry.ActorId}:{entry.TimestampUtc:O}:{entry.PreviousValue}:{entry.NewValue}:{entry.ChangeType}";
                entry.ChainedHashSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dataToHash))).ToLowerInvariant();
            }

            list.Add(entry);
        }
    }

    public IEnumerable<AuditEntry> GetHistory(string centerId, string? resourceId = null)
    {
        if (HistoryByCenter.TryGetValue(centerId, out var list))
        {
            lock (LockObj)
            {
                return list.ToList();
            }
        }

        return Enumerable.Empty<AuditEntry>();
    }
}

public class RedStubHealthCheckService : IHealthCheckService
{
    public (string status, long responseTimeMs) CheckLiveness()
    {
        return ("Healthy", 15);
    }

    public (string status, Dictionary<string, string> componentDetails) CheckReadiness()
    {
        var details = new Dictionary<string, string>
        {
            ["database"] = "Connected",
            ["memory"] = "Optimal",
            ["enclaves"] = "Active"
        };

        return ("Healthy", details);
    }
}

public class RedStubEscandalloRepository : IEscandalloRepository
{
    private static readonly ConcurrentDictionary<string, List<CompleteEscandallo>> Services = new();

    public IEnumerable<CompleteEscandallo> GetByCenter(string centerId)
    {
        return Services.GetOrAdd(centerId, _ => new List<CompleteEscandallo>()).ToList();
    }

    public CompleteEscandallo? GetById(string centerId, string id)
    {
        return GetByCenter(centerId).FirstOrDefault(x => x.Id == id);
    }

    public void Save(string centerId, CompleteEscandallo escandallo)
    {
        var list = Services.GetOrAdd(centerId, _ => new List<CompleteEscandallo>());
        int idx = list.FindIndex(x => x.Id == escandallo.Id);
        if (idx >= 0) list[idx] = escandallo;
        else list.Add(escandallo);
    }

    public bool Delete(string centerId, string id)
    {
        if (Services.TryGetValue(centerId, out var list))
        {
            return list.RemoveAll(x => x.Id == id) > 0;
        }
        return false;
    }

    public int Clear(string centerId)
    {
        if (Services.TryGetValue(centerId, out var list))
        {
            int count = list.Count;
            list.Clear();
            return count;
        }
        return 0;
    }
}

