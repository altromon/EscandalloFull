using System.Text.Json;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Security;

public class GuestDataCensorshipTests
{
    [Fact]
    public void Nominal_GuestEscandalloDto_OmitsWagesRentsAndFacilityCostsInSerializedJson()
    {
        // Arrange: Prepare complete vs guest DTOs
        var complete = new CompleteEscandallo
        {
            Id = "esc-001",
            ServiceName = "Mesodermolifting",
            DurationMinutes = 60,
            WorkerCategory = "Encargado",
            LaborCost = 16.36m,
            OverheadCost = 4.97m,
            CosmeticsCost = 13.38m,
            TotalEscandalloCost = 34.71m,
            AppliedPvr = 63.00m,
            NetMarginAbsolute = 17.36m,
            NetMarginPercentage = 33.3m,
            TrafficLightColor = TrafficLightColor.Amarillo,
            Consumables = new List<ConsumableUsage>
            {
                new() { ProductId = "cos-1", ProductName = "Cleasing Milk", AmountUsed = 10, UnitOfMeasure = UnitOfMeasure.Ml, ComputedCost = 0.72m }
            }
        };

        var guest = new GuestEscandallo
        {
            Id = "esc-001",
            ServiceName = "Mesodermolifting",
            DurationMinutes = 60,
            TrafficLightColor = TrafficLightColor.Amarillo,
            IsActive = true,
            Consumables = new List<GuestConsumableUsage>
            {
                new() { ProductId = "cos-1", ProductName = "Cleasing Milk", AmountUsed = 10, UnitOfMeasure = UnitOfMeasure.Ml }
            }
        };

        // Act: Serialize guest DTO
        string guestJson = JsonSerializer.Serialize(guest);

        // Assert: SEC-REQ-DATA-CENSOR-GUEST-001 and BR-GUEST-CENSOR-001 verification
        Assert.DoesNotContain("LaborCost", guestJson);
        Assert.DoesNotContain("OverheadCost", guestJson);
        Assert.DoesNotContain("TotalEscandalloCost", guestJson);
        Assert.DoesNotContain("worker_hourly_wage", guestJson);
        Assert.DoesNotContain("facility_rent", guestJson);
        Assert.DoesNotContain("16.36", guestJson); // Wage must not leak
        Assert.DoesNotContain("4.97", guestJson);  // Facility rent must not leak

        // Allowed fields must exist
        Assert.Contains("Mesodermolifting", guestJson);
        Assert.Contains("Cleasing Milk", guestJson);
        Assert.Contains("10", guestJson); // Amount used
    }
}
