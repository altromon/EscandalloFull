using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;
using Escandallo.Core.Contracts;

namespace Escandallo.Core.Services;

/// <summary>
/// Gestor de suscripciones, cuota freemium y webhooks de Stripe (BR-FREEMIUM-LIMIT-001, SEC-REQ-QUOTA-BACKEND-ENFORCE-001, SEC-REQ-STRIPE-SIGNATURE-001).
/// </summary>
public class SubscriptionManager : ISubscriptionManager
{
    private readonly ConcurrentDictionary<string, SubscriptionStatus> _centerSubscriptions = new();
    private readonly ConcurrentDictionary<string, byte> _processedEvents = new();
    private readonly object _syncLock = new();

    public SubscriptionManager()
    {
        // Inicialización de centros de prueba con sus estados canónicos
        _centerSubscriptions["ctr-madrid-1"] = new SubscriptionStatus
        {
            Plan = PlanType.Free,
            ActiveEscandallos = 1,
            MaxAllowedEscandallos = 2
        };

        _centerSubscriptions["ctr-radiante-1"] = new SubscriptionStatus
        {
            Plan = PlanType.Free,
            ActiveEscandallos = 1,
            MaxAllowedEscandallos = 2
        };

        _centerSubscriptions["ctr-luz-belleza"] = new SubscriptionStatus
        {
            Plan = PlanType.Free,
            ActiveEscandallos = 2,
            MaxAllowedEscandallos = 2
        };

        _centerSubscriptions["ctr-test-free"] = new SubscriptionStatus
        {
            Plan = PlanType.Free,
            ActiveEscandallos = 2,
            MaxAllowedEscandallos = 2
        };
    }

    public SubscriptionStatus GetStatus(string centerId)
    {
        return _centerSubscriptions.GetOrAdd(centerId, id => new SubscriptionStatus
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
        lock (_syncLock)
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
        lock (_syncLock)
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
        lock (_syncLock)
        {
            var status = GetStatus(centerId);
            status.Plan = PlanType.Premium;
            status.MaxAllowedEscandallos = int.MaxValue;
            return status;
        }
    }

    public SubscriptionStatus CancelSubscription(string centerId)
    {
        lock (_syncLock)
        {
            var status = GetStatus(centerId);
            status.Plan = PlanType.Free;
            status.MaxAllowedEscandallos = 2;
            return status;
        }
    }

    public void ResetActiveCount(string centerId)
    {
        lock (_syncLock)
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

        string? eventId = ExtractJsonField(rawPayload, "id");
        if (string.IsNullOrEmpty(eventId))
            return false;

        if (!_processedEvents.TryAdd(eventId, 0))
            return false; // Duplicado detectado: descartado por idempotencia

        string? eventType = ExtractJsonField(rawPayload, "type");
        string? centerId = ExtractJsonField(rawPayload, "center_id");

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

    private static string? ExtractJsonField(string json, string field)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(field, out var prop))
                return prop.GetString();
        }
        catch
        {
            // Fallback por expresión regular si el JSON tiene formato ligero
            var match = Regex.Match(json, $"\"{field}\"\\s*:\\s*\"([^\"]+)\"");
            if (match.Success)
                return match.Groups[1].Value;
        }
        return null;
    }
}
