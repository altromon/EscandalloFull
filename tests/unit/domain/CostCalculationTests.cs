using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Domain;

public class CostCalculationTests
{
    private readonly IEscandalloCostEngine _costEngine = new EscandalloCostEngine();
    private readonly IMarginCalculator _marginCalculator = new MarginCalculator();

    [Fact]
    public void Nominal_CalculateMesodermolifting_StandardEscandalloAndPvp()
    {
        // Arrange
        // 60 min, 0.08286 EUR/min overhead, 0.27273 EUR/min labor, 13.37723 EUR cosmetics
        int duration = 60;
        decimal overheadPerMin = 0.08285985m;
        decimal workerPerMin = 0.27272727m;
        decimal cosmeticsCost = 13.37723m;

        // Act
        decimal totalEscandallo = _costEngine.CalculateEscandallo(duration, overheadPerMin, workerPerMin, cosmeticsCost);
        var (suggestedBase, suggestedPvp) = _marginCalculator.CalculateSuggestedPrices(totalEscandallo, 50.0m, 21.0m);

        // Assert
        Assert.Equal(34.71m, Math.Round(totalEscandallo, 2));
        Assert.Equal(52.07m, Math.Round(suggestedBase, 2));
        Assert.Equal(63.00m, Math.Round(suggestedPvp, 2));
    }

    [Fact]
    public void Nominal_CalculateMedicalDermatology_ExemptFromVat()
    {
        // Arrange
        decimal totalEscandallo = 30.00m;
        decimal appliedPvr = 80.00m;
        decimal taxRate = 0.0m; // Art. 20 Ley IVA

        // Act
        var (netMarginAbs, netMarginPct) = _marginCalculator.CalculateRealMargin(appliedPvr, totalEscandallo, taxRate);
        var color = _marginCalculator.DetermineTrafficLight(netMarginPct);

        // Assert
        Assert.Equal(50.00m, netMarginAbs);
        Assert.Equal(62.5m, netMarginPct);
        Assert.Equal(TrafficLightColor.Verde, color);
    }

    [Theory]
    [InlineData(5, 400.00, 66.12, 47.5, TrafficLightColor.Verde)]
    [InlineData(5, 300.00, 49.59, 30.0, TrafficLightColor.Amarillo)]
    [InlineData(5, 200.00, 33.06, -5.0, TrafficLightColor.Rojo)]
    [InlineData(1, 63.00, 52.07, 33.3, TrafficLightColor.Amarillo)]
    public void Boundary_CalculateSessionPackMargins(int sessions, decimal totalPackPrice, decimal expectedSessionRevenue, decimal expectedMarginPct, TrafficLightColor expectedColor)
    {
        // Arrange
        decimal totalEscandallo = 34.71m;
        decimal taxRate = 21.0m;

        // Act
        var (sessionRevenue, marginPct) = _marginCalculator.CalculatePackMargin(totalPackPrice, sessions, totalEscandallo, taxRate);
        var color = _marginCalculator.DetermineTrafficLight(marginPct);

        // Assert
        Assert.Equal(expectedSessionRevenue, Math.Round(sessionRevenue, 2));
        Assert.Equal(expectedMarginPct, Math.Round(marginPct, 1));
        Assert.Equal(expectedColor, color);
    }

    [Fact]
    public void LossDetection_SellingBelowCost_EmitsExplicitWarningAndRedTrafficLight()
    {
        // Arrange
        decimal totalEscandallo = 34.71m;
        decimal appliedPvr = 30.00m;
        decimal taxRate = 21.0m;

        // Act
        var (netMarginAbs, netMarginPct) = _marginCalculator.CalculateRealMargin(appliedPvr, totalEscandallo, taxRate);
        var color = _marginCalculator.DetermineTrafficLight(netMarginPct);
        string? warning = _marginCalculator.FormatLossWarning(netMarginAbs);

        // Assert
        Assert.Equal(-9.92m, Math.Round(netMarginAbs, 2));
        Assert.Equal(-40.0m, Math.Round(netMarginPct, 1));
        Assert.Equal(TrafficLightColor.Rojo, color);
        Assert.NotNull(warning);
        Assert.Contains("Atención: Estás perdiendo 9.92 € por cada sesión", warning);
    }

    [Theory]
    [InlineData(0, 10.0, typeof(ArgumentOutOfRangeException))]
    [InlineData(-10, 10.0, typeof(ArgumentOutOfRangeException))]
    [InlineData(60, -5.0, typeof(ArgumentOutOfRangeException))]
    public void OutOfRange_InvalidEscandalloParameters_ThrowsException(int duration, decimal cosmeticsCost, Type expectedException)
    {
        // Act & Assert
        Assert.Throws(expectedException, () =>
        {
            _costEngine.CalculateEscandallo(duration, 0.08m, 0.27m, cosmeticsCost);
        });
    }
}
