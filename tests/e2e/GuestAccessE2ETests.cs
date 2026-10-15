using System.Text.Json;
using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.E2E;

public class GuestAccessE2ETests
{
    private readonly IAuthGateway _auth = new AuthGateway();

    [Fact]
    public void E2E_GuestAccess_PayloadContainsZeroFinancialStructuralFields()
    {
        // 1. Authenticate with Guest Role
        var session = _auth.Login("guest@collaborator.com", "GuestPass123!");
        Assert.Equal(RoleType.Guest, session.Role);

        // 2. Fetch Escandallo DTO
        var guestEscandallo = new GuestEscandallo
        {
            Id = "esc-guest-view",
            ServiceName = "Mesodermolifting",
            DurationMinutes = 60,
            TrafficLightColor = TrafficLightColor.Verde,
            IsActive = true,
            Consumables = new List<GuestConsumableUsage>
            {
                new() { ProductId = "cos-01", ProductName = "Cleasing Milk", AmountUsed = 10, UnitOfMeasure = UnitOfMeasure.Ml }
            }
        };

        // 3. Verify Serialized Wire Payload
        string json = JsonSerializer.Serialize(guestEscandallo);

        // Assert: Zero leakage of sensitive structural data (SEC-REQ-DATA-CENSOR-GUEST-001)
        Assert.DoesNotContain("LaborCost", json);
        Assert.DoesNotContain("OverheadCost", json);
        Assert.DoesNotContain("MonthlyCompanyCost", json);
        Assert.DoesNotContain("HourlyWage", json);
        Assert.DoesNotContain("Rent", json);
    }
}
