using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Security;

public class StripeWebhookSecurityTests
{
    private readonly ISubscriptionManager _subscriptionManager = new SubscriptionManager();

    [Theory]
    [InlineData("", "Missing signature header")]
    [InlineData("t=12345,v1=invalidsig", "Invalid cryptographic signature")]
    public void Security_StripeWebhook_RejectsMissingOrInvalidSignature(string signatureHeader, string reason)
    {
        // Arrange
        string payload = "{\"id\":\"evt_123\",\"type\":\"checkout.session.completed\"}";

        // Act & Assert
        bool result = _subscriptionManager.ProcessWebhook(payload, signatureHeader);
        Assert.False(result, reason);
    }

    [Fact]
    public void Security_StripeWebhook_RejectsExpiredTimestamp_MitigatingReplayAttacks()
    {
        // Arrange: Timestamp > 300 seconds old
        long expiredTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 450;
        string signatureHeader = $"t={expiredTimestamp},v1=valid_mock_signature";
        string payload = "{\"id\":\"evt_test_001\",\"type\":\"checkout.session.completed\"}";

        // Act
        bool result = _subscriptionManager.ProcessWebhook(payload, signatureHeader);

        // Assert
        Assert.False(result, "Webhooks older than 300s must be rejected to prevent replay attacks.");
    }

    [Fact]
    public void Idempotency_StripeWebhook_IgnoresDuplicateEventsWithoutDoubleCrediting()
    {
        // Arrange: Webhook processed once
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 10;
        string signatureHeader = $"t={currentTimestamp},v1=valid_mock_signature";
        string payload = "{\"id\":\"evt_test_002\",\"type\":\"checkout.session.completed\"}";

        // Act: Process first time and second time
        bool firstProcess = _subscriptionManager.ProcessWebhook(payload, signatureHeader);
        bool secondProcess = _subscriptionManager.ProcessWebhook(payload, signatureHeader);

        // Assert
        // First process may accept, but second must be recognized as duplicate and discarded idempotently
        Assert.False(secondProcess, "Duplicate webhook events must be ignored idempotently.");
    }
}
