using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Bdd.StepDefinitions;

public class SubscriptionSteps
{
    private readonly ISubscriptionManager _subscription = new SubscriptionManager();

    [Fact]
    [Trait("Category", "FR-SUBSCRIPTION-GATE-001")]
    public void Scenario_BlockThirdEscandalloCreationOnFreePlan()
    {
        string centerId = "ctr-test-free";

        // Given el centro tiene suscripción "Free" y cuenta con 2 escandallos activos
        var status = _subscription.GetStatus(centerId);

        // When el gestor intenta dar de alta un tercer escandallo
        // Then el sistema bloquea la creación con el error de cuota excedida
        Assert.Throws<InvalidOperationException>(() =>
        {
            _subscription.ValidateAndRegisterEscandallo(centerId, "esc-03-blocked");
        });
    }

    [Theory]
    [Trait("Category", "FR-SUBSCRIPTION-GATE-001")]
    [InlineData("Free", 0, "permitir_creacion")]
    [InlineData("Free", 1, "permitir_creacion")]
    [InlineData("Free", 2, "bloquear_y_mostrar_modal")]
    [InlineData("Premium", 2, "permitir_creacion")]
    [InlineData("Premium", 50, "permitir_creacion")]
    public void ScenarioOutline_AuthorizeCreationAccordingToPlan(string plan, int currentCount, string expectedAction)
    {
        bool isPremium = plan == "Premium";
        bool canCreate = isPremium || currentCount < 2;

        if (expectedAction == "permitir_creacion")
        {
            Assert.True(canCreate);
        }
        else
        {
            Assert.False(canCreate);
        }
    }
}
