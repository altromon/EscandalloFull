using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Domain;

public class CostFactorsValidationTests
{
    private readonly IEscandalloCostEngine _costEngine = new EscandalloCostEngine();

    [Fact]
    public void Nominal_OpeningHours_Standard176Hours_ConvertsTo10560Minutes()
    {
        // Arrange
        decimal hours = 176m;
        decimal rent = 335.00m;
        decimal supplies = 100.00m;
        decimal consumables = 440.00m;

        // Act
        var parameters = _costEngine.CalculateCostParameters(hours, rent, supplies, consumables);

        // Assert
        Assert.Equal(176m, parameters.MonthlyOpeningHours);
        Assert.Equal(10560m, parameters.MonthlyOpeningMinutes);
        Assert.Equal(875.00m, parameters.MonthlyOverheadCost);
        Assert.Equal(0.08286m, Math.Round(parameters.OverheadCostPerMinute, 5));
    }

    [Fact]
    public void Nominal_CosmeticProduct_CleasingMilk_ComputesGrossAndEffectiveCostsWithWaste()
    {
        // Arrange
        decimal packagePrice = 34.51m;
        decimal capacity = 500m; // ml
        decimal wastePercentage = 5.0m; // 5% in tanto por 100

        // Act
        var (grossUnitCost, effectiveUnitCost) = _costEngine.CalculateCosmeticUnitCost(packagePrice, capacity, wastePercentage);

        // Assert
        Assert.Equal(0.06902m, Math.Round(grossUnitCost, 5));
        Assert.Equal(0.07247m, Math.Round(effectiveUnitCost, 5));
    }

    [Theory]
    [InlineData(0, typeof(ArgumentOutOfRangeException))]
    [InlineData(-10, typeof(ArgumentOutOfRangeException))]
    [InlineData(745, typeof(ArgumentOutOfRangeException))] // Exceeds max possible hours in a 31-day month
    public void OutOfRange_OpeningHours_InvalidBoundaries_ThrowsException(decimal hours, Type expectedException)
    {
        Assert.Throws(expectedException, () =>
        {
            _costEngine.CalculateCostParameters(hours, 335m, 100m, 440m);
        });
    }

    [Theory]
    [InlineData(-10.0, 500, 5.0, typeof(ArgumentOutOfRangeException))]
    [InlineData(34.51, 0, 5.0, typeof(DivideByZeroException))]
    [InlineData(34.51, -500, 5.0, typeof(ArgumentOutOfRangeException))]
    [InlineData(34.51, 500, -1.0, typeof(ArgumentOutOfRangeException))]
    [InlineData(34.51, 500, 105.0, typeof(ArgumentOutOfRangeException))]
    public void OutOfRange_CosmeticProduct_InvalidInputs_ThrowsException(decimal price, decimal capacity, decimal waste, Type expectedException)
    {
        Assert.Throws(expectedException, () =>
        {
            _costEngine.CalculateCosmeticUnitCost(price, capacity, waste);
        });
    }
}
