using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.E2E;

public class AvailabilityHealthCheckE2ETests
{
    private readonly IHealthCheckService _healthCheck = new HealthCheckService();

    [Fact]
    [Trait("Quality", "QR-AVAILABILITY-001")]
    public void E2E_24_7_AvailabilityMonitoring_ProbesRespondHealthy()
    {
        // 1. Liveness probe (/healthz)
        var (livenessStatus, responseTime) = _healthCheck.CheckLiveness();
        Assert.Equal("Healthy", livenessStatus);
        Assert.True(responseTime < 200);

        // 2. Readiness probe (/readyz)
        var (readinessStatus, details) = _healthCheck.CheckReadiness();
        Assert.Equal("Healthy", readinessStatus);
        Assert.Equal("Connected", details["database"]);
    }
}
