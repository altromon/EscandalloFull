using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Bdd.StepDefinitions;

public class SecuritySteps
{
    private readonly IAuthGateway _auth = new AuthGateway();
    private readonly ISubscriptionManager _subscription = new SubscriptionManager();
    private readonly IExportDocumentEngine _export = new ExportDocumentEngine();

    [Fact]
    [Trait("Category", "SEC-REQ-AUTH-MEMBER-001")]
    [Trait("Security", "Mitigation")]
    public void Scenario_RejectionOfCenterSelectionNotAssignedToUser()
    {
        // When el usuario envía una petición para seleccionar un centro no asignado
        Assert.Throws<UnauthorizedAccessException>(() =>
        {
            _auth.SelectCenter("usr-100", "ctr-barcelona-9");
        });
    }

    [Fact]
    [Trait("Category", "SEC-REQ-STRIPE-SIGNATURE-001")]
    [Trait("Security", "Mitigation")]
    public void Scenario_RejectionOfStripeWebhookWithMissingOrInvalidSignature()
    {
        string payload = "{\"type\":\"checkout.session.completed\"}";
        bool accepted = _subscription.ProcessWebhook(payload, "invalid_sig");
        Assert.False(accepted);
    }

    [Fact]
    [Trait("Category", "SEC-REQ-EXPORT-SANITIZATION-001")]
    [Trait("Security", "Mitigation")]
    public void Scenario_NeutralizationOfFormulaInjectionInExportedCell()
    {
        string input = "=cmd|'/C calc'!A0";
        string sanitized = _export.SanitizeCell(input);
        Assert.Equal("'=cmd|'/C calc'!A0", sanitized);
    }
}
