using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/billing")]
public class BillingController : ControllerBase
{
    private readonly ISubscriptionManager _subscriptionManager;

    public BillingController(ISubscriptionManager subscriptionManager)
    {
        _subscriptionManager = subscriptionManager;
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
}
