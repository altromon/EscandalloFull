using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.E2E;

public class FreemiumToStripeUpgradeE2ETests
{
    private readonly ISubscriptionManager _subscription = new SubscriptionManager();

    [Fact]
    public void E2E_FreemiumToStripeUpgrade_UnlocksUnlimitedQuotaAfterWebhook()
    {
        string centerId = "ctr-luz-belleza";

        // 1. Initial State: Free with 2 active escandallos
        var initialStatus = _subscription.GetStatus(centerId);
        Assert.Equal(PlanType.Free, initialStatus.Plan);
        Assert.Equal(2, initialStatus.ActiveEscandallos);
        Assert.False(initialStatus.CanCreateMore);

        // 2. Attempt to register 3rd escandallo -> Throws InvalidOperationException (QUOTA_EXCEEDED)
        Assert.Throws<InvalidOperationException>(() =>
        {
            _subscription.ValidateAndRegisterEscandallo(centerId, "esc-03-blocked");
        });

        // 3. Initiate Checkout Session
        var checkoutRequest = new CreateCheckoutRequest
        {
            PlanBillingCycle = "monthly",
            SuccessUrl = "https://app.escandallo.com/billing/success",
            CancelUrl = "https://app.escandallo.com/billing/cancel"
        };
        string checkoutUrl = _subscription.CreateCheckoutSession(centerId, checkoutRequest);
        Assert.NotEmpty(checkoutUrl);

        // 4. Ingest successful Stripe webhook
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string webhookPayload = $"{{\"id\":\"evt_checkout_success\",\"type\":\"checkout.session.completed\",\"center_id\":\"{centerId}\",\"plan\":\"Premium\"}}";
        string validSignature = $"t={now},v1=mock_signature_hash";

        bool webhookProcessed = _subscription.ProcessWebhook(webhookPayload, validSignature);
        Assert.True(webhookProcessed);

        // 5. Post-Condition: Quota upgraded to Premium and allows unlimited creations
        var upgradedStatus = _subscription.GetStatus(centerId);
        Assert.Equal(PlanType.Premium, upgradedStatus.Plan);
        Assert.True(upgradedStatus.CanCreateMore);

        // 6. 3rd escandallo now registers successfully
        _subscription.ValidateAndRegisterEscandallo(centerId, "esc-03-unlocked");
    }
}
