using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/billing")]
public class BillingController : ControllerBase
{
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly IAuditTrailRepository? _auditTrailRepo;

    public BillingController(ISubscriptionManager subscriptionManager, IAuditTrailRepository? auditTrailRepo = null)
    {
        _subscriptionManager = subscriptionManager;
        _auditTrailRepo = auditTrailRepo;
    }

    [HttpGet("status")]
    [HttpGet("status/{centerId}")]
    public ActionResult<SubscriptionStatus> GetStatus(string? centerId)
    {
        string targetCenter = centerId ?? Request.Query["centerId"].FirstOrDefault() ?? "ctr-madrid-1";
        var status = _subscriptionManager.GetStatus(targetCenter);
        return Ok(status);
    }

    [HttpPost("checkout")]
    public ActionResult CreateCheckout([FromQuery] string? centerId, [FromBody] CreateCheckoutRequest request)
    {
        string targetCenter = centerId ?? "ctr-madrid-1";
        string url = _subscriptionManager.CreateCheckoutSession(targetCenter, request);
        return Ok(new { checkoutUrl = url });
    }

    [HttpPost("stripe-webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        string? signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        using var reader = new StreamReader(Request.Body);
        string payload = await reader.ReadToEndAsync();

        bool processed = _subscriptionManager.ProcessWebhook(payload, signature ?? string.Empty);
        if (!processed)
        {
            return BadRequest(new { message = "Webhook descartado o firma inválida." });
        }

        return Ok(new { received = true });
    }

    [HttpPost("mock-complete-checkout")]
    public ActionResult<SubscriptionStatus> MockCompleteCheckout(
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var status = _subscriptionManager.UpgradeToPremium(targetCenter);
        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = 0,
            NewValue = 29.99m,
            ChangeType = "UPGRADE_PLAN_PRO:Suscripción Ilimitada Activada"
        });
        return Ok(status);
    }

    [HttpPost("mock-cancel-subscription")]
    public ActionResult<SubscriptionStatus> MockCancelSubscription(
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var status = _subscriptionManager.CancelSubscription(targetCenter);
        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = 29.99m,
            NewValue = 0,
            ChangeType = "CANCEL_PLAN_PRO:Cancelación voluntaria a Plan Gratuito"
        });
        return Ok(status);
    }
}
