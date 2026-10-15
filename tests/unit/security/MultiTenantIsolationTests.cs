using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Security;

public class MultiTenantIsolationTests
{
    private readonly IAuthGateway _authGateway = new AuthGateway();

    [Fact]
    public void Security_CrossCenterAccess_RejectsUnauthorizedCenterSelectionWith403()
    {
        // Arrange
        string userId = "usr-100";
        string unauthorizedCenterId = "ctr-barcelona-9";

        // Act & Assert
        // Attempting to select center outside user's assigned tenant/center list
        Assert.Throws<UnauthorizedAccessException>(() =>
        {
            _authGateway.SelectCenter(userId, unauthorizedCenterId);
        });
    }

    [Fact]
    public void Security_TamperedToken_RejectsSessionWithInvalidCenterId()
    {
        // Arrange
        string tamperedToken = "header.payload_with_ctr_ajeno.invalidsignature";
        string expectedCenterId = "ctr-madrid-1";

        // Act
        bool isValid = _authGateway.ValidateSession(tamperedToken, expectedCenterId);

        // Assert
        Assert.False(isValid);
    }
}
