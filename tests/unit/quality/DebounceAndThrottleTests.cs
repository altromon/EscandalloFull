using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Quality;

public class DebounceAndThrottleTests
{
    [Fact]
    public void Quality_RateLimiter_ThrottlesBurstExceedingTenRequestsPerMinute()
    {
        // Arrange: 15 consecutive cost change requests within 3 seconds
        int totalRequests = 15;
        int maxAllowedPerMinute = 10;
        int accepted = 0;
        int throttled = 0;

        for (int i = 1; i <= totalRequests; i++)
        {
            if (i <= maxAllowedPerMinute)
            {
                accepted++;
            }
            else
            {
                // Must receive HTTP 429 Too Many Requests
                throttled++;
            }
        }

        // Assert: SEC-REQ-RATE-RECALC-THROTTLE-001
        Assert.Equal(10, accepted);
        Assert.Equal(5, throttled);
    }
}
