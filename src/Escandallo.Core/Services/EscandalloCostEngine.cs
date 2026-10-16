using Escandallo.Core.Contracts;

namespace Escandallo.Core.Services;

/// <summary>
/// Motor de cálculo de factores de coste y escandallos (BR-COST-FORMULA-001, CMP-COST-ENGINE-001).
/// </summary>
public class EscandalloCostEngine : IEscandalloCostEngine
{
    private const decimal MaxMonthlyHours = 744m; // 31 días * 24 horas

    public CenterCostParameters CalculateCostParameters(
        decimal monthlyOpeningHours,
        decimal localRent,
        decimal supplies,
        decimal generalCabinConsumables)
    {
        if (monthlyOpeningHours <= 0 || monthlyOpeningHours > MaxMonthlyHours)
            throw new ArgumentOutOfRangeException(nameof(monthlyOpeningHours), "Horas mensuales fuera de rango válido.");

        if (localRent < 0 || supplies < 0 || generalCabinConsumables < 0)
            throw new ArgumentOutOfRangeException("Los costes fijos no pueden ser negativos.");

        decimal totalOverhead = localRent + supplies + generalCabinConsumables;

        return new CenterCostParameters
        {
            MonthlyOpeningHours = monthlyOpeningHours,
            LocalRent = localRent,
            Supplies = supplies,
            GeneralCabinConsumables = generalCabinConsumables,
            MonthlyOverheadCost = totalOverhead
        };
    }

    public (decimal grossUnitCost, decimal effectiveUnitCost) CalculateCosmeticUnitCost(
        decimal packagePrice,
        decimal capacity,
        decimal wastePercentage)
    {
        if (packagePrice < 0)
            throw new ArgumentOutOfRangeException(nameof(packagePrice), "El precio del envase no puede ser negativo.");

        if (capacity == 0)
            throw new DivideByZeroException("La capacidad del envase no puede ser cero.");

        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "La capacidad no puede ser negativa.");

        if (wastePercentage < 0 || wastePercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(wastePercentage), "La merma debe estar entre 0% y 100%.");

        decimal grossUnitCost = packagePrice / capacity;
        decimal effectiveUnitCost = grossUnitCost * (1m + (wastePercentage / 100m));

        return (grossUnitCost, effectiveUnitCost);
    }

    public decimal CalculateEscandallo(
        int durationMinutes,
        decimal overheadPerMinute,
        decimal workerCostPerMinute,
        decimal totalCosmeticsCost)
    {
        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "La duración debe ser mayor que cero.");

        if (totalCosmeticsCost < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCosmeticsCost), "El coste de cosméticos no puede ser negativo.");

        if (overheadPerMinute < 0)
            throw new ArgumentOutOfRangeException(nameof(overheadPerMinute), "El coste indirecto no puede ser negativo.");

        if (workerCostPerMinute < 0)
            throw new ArgumentOutOfRangeException(nameof(workerCostPerMinute), "El coste de personal no puede ser negativo.");

        decimal laborCost = durationMinutes * workerCostPerMinute;
        decimal overheadCost = durationMinutes * overheadPerMinute;

        return laborCost + overheadCost + totalCosmeticsCost;
    }
}
