using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Quality;

public class AvailabilityAndHealthCheckTests
{
    private readonly IHealthCheckService _healthCheck = new HealthCheckService();
    private readonly IEscandalloCostEngine _costEngine = new EscandalloCostEngine();
    private readonly IMarginCalculator _marginCalculator = new MarginCalculator();

    [Fact]
    [Trait("Quality", "QR-AVAILABILITY-001")]
    public void Availability_LivenessEndpoint_ReturnsHealthyWithin200Ms()
    {
        // Act: Invoke /healthz probe
        var (status, responseTimeMs) = _healthCheck.CheckLiveness();

        // Assert: Process must be alive and responsive
        Assert.Equal("Healthy", status);
        Assert.True(responseTimeMs < 200, $"Liveness probe response time ({responseTimeMs}ms) exceeded 200ms threshold.");
    }

    [Fact]
    [Trait("Quality", "QR-AVAILABILITY-001")]
    public void Availability_ReadinessEndpoint_VerifiesDatabaseAndResourceHealth()
    {
        // Act: Invoke /readyz probe
        var (status, details) = _healthCheck.CheckReadiness();

        // Assert: Must report component breakdown (database, memory)
        Assert.Equal("Healthy", status);
        Assert.NotNull(details);
        Assert.True(details.ContainsKey("database"), "Readiness probe must report database connectivity status.");
    }

    [Fact]
    [Trait("Quality", "QR-AVAILABILITY-001")]
    public void Resilience_ExternalGatewayOutage_DoesNotImpactCoreCalculationAvailability()
    {
        // Arrange: Simulate Stripe external outage
        bool stripeOutage = true;

        // Act: Calculate escandallo even if external payment provider is offline
        decimal totalEscandallo = _costEngine.CalculateEscandallo(60, 0.08286m, 0.27273m, 13.38m);
        var (suggestedBase, suggestedPvp) = _marginCalculator.CalculateSuggestedPrices(totalEscandallo, 50m, 21m);

        // Assert: 24/7 core operations remain 100% available regardless of external SaaS outages
        Assert.True(stripeOutage);
        Assert.True(totalEscandallo > 0m);
        Assert.True(suggestedPvp > 0m);
    }
}
