using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Bdd.StepDefinitions;

public class CostConfigSteps
{
    private readonly IEscandalloCostEngine _costEngine = new EscandalloCostEngine();

    [Fact]
    [Trait("Category", "FR-COST-CONFIG-001")]
    public void Scenario_RegisterCosmeticProductWithWastePercentage()
    {
        // When el gestor registra un cosmético con nombre "Cleasing Milk", precio 34.51 EUR, capacidad 500 "ml" y merma 5%
        var (grossCost, effectiveCost) = _costEngine.CalculateCosmeticUnitCost(34.51m, 500m, 5.0m);

        // Then calcula un coste unitario bruto de 0.06902 EUR por "ml"
        Assert.Equal(0.06902m, Math.Round(grossCost, 5));

        // And calcula un coste unitario efectivo de 0.07247 EUR por "ml"
        Assert.Equal(0.07247m, Math.Round(effectiveCost, 5));
    }

    [Theory]
    [Trait("Category", "FR-COST-CONFIG-001")]
    [InlineData("trabajador_mes", 1980.00, "euros/mes", "valido")]
    [InlineData("gastos_local_mes", 335.00, "euros/mes", "valido")]
    [InlineData("apertura_horas", 0, "h/mes", "error_cero")]
    [InlineData("apertura_horas", -10, "h/mes", "error_negativo")]
    [InlineData("producto_coste", -10.00, "euros/envase", "error_negativo")]
    public void ScenarioOutline_ValidateInputDataOnCostFactors(string type, decimal value, string units, string expectedStatus)
    {
        Assert.False(string.IsNullOrWhiteSpace(units));
        if (expectedStatus == "valido")
        {
            Assert.True(value > 0);
        }
        else if (expectedStatus == "error_cero")
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _costEngine.CalculateCostParameters(value, 335m, 100m, 440m);
            });
        }
        else if (expectedStatus == "error_negativo")
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                if (type == "apertura_horas")
                    _costEngine.CalculateCostParameters(value, 335m, 100m, 440m);
                else
                    _costEngine.CalculateCosmeticUnitCost(value, 500m, 5m);
            });
        }
    }
}
