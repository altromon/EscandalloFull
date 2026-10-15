using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Security;

public class ConcurrencyQuotaTests
{
    private readonly ISubscriptionManager _subscriptionManager = new SubscriptionManager();

    [Fact]
    public async Task Security_ConcurrentCreations_MitigatesToctouRaceCondition_EnforcingMaxTwoActive()
    {
        // Arrange
        string centerId = "ctr-radiante-1";
        // Pre-condition: Free center already has 1 active escandallo
        int numberOfConcurrentRequests = 10;
        int successfulCreations = 0;
        int rejectedCreations = 0;

        // Act: Simulate 10 parallel requests attempting to create escandallos
        var tasks = Enumerable.Range(1, numberOfConcurrentRequests).Select(async i =>
        {
            try
            {
                // Each request attempts to validate and register an active escandallo
                _subscriptionManager.ValidateAndRegisterEscandallo(centerId, $"esc-test-{i}");
                Interlocked.Increment(ref successfulCreations);
            }
            catch (InvalidOperationException)
            {
                // Expected rejection: QUOTA_EXCEEDED
                Interlocked.Increment(ref rejectedCreations);
            }
        });

        await Task.WhenAll(tasks);

        // Assert: Under serializable/locking transaction, exactly 1 must succeed and 9 must be rejected
        Assert.Equal(1, successfulCreations);
        Assert.Equal(9, rejectedCreations);

        var finalStatus = _subscriptionManager.GetStatus(centerId);
        Assert.Equal(2, finalStatus.ActiveEscandallos);
        Assert.False(finalStatus.CanCreateMore);
    }
}
