using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Domain;

public class ColorThresholdsTests
{
    private readonly IMarginCalculator _marginCalculator = new MarginCalculator();

    [Theory]
    [InlineData(100.0, TrafficLightColor.Verde)]
    [InlineData(50.0, TrafficLightColor.Verde)]
    [InlineData(40.0, TrafficLightColor.Verde)]
    [InlineData(39.99, TrafficLightColor.Amarillo)]
    [InlineData(30.0, TrafficLightColor.Amarillo)]
    [InlineData(20.0, TrafficLightColor.Amarillo)]
    [InlineData(19.99, TrafficLightColor.Rojo)]
    [InlineData(5.0, TrafficLightColor.Rojo)]
    [InlineData(0.0, TrafficLightColor.Rojo)]
    [InlineData(-10.0, TrafficLightColor.Rojo)]
    public void Boundary_DetermineTrafficLight_StrictThresholdConformity(decimal marginPercentage, TrafficLightColor expectedColor)
    {
        // Act
        var color = _marginCalculator.DetermineTrafficLight(marginPercentage);

        // Assert
        Assert.Equal(expectedColor, color);
    }
}
