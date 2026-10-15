using System.Collections.Concurrent;
using Escandallo.Core.Contracts;

namespace Escandallo.Core.Services;

/// <summary>
/// Repositorio de configuración de parámetros de costes por centro y tarifas de personal (FR-COST-CONFIG-001).
/// </summary>
public class CostConfigurationRepository : ICostConfigurationRepository
{
    public const decimal DefaultMarkupPercentage = 50.0m;

    private readonly ConcurrentDictionary<string, CenterCostParameters> _centerParams = new();
    private readonly Dictionary<string, decimal> _workerRates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Encargado"] = 0.272727m,
        ["Esteticista"] = 0.272727m,
        ["Auxiliar"] = 0.227273m
    };

    public CostConfigurationRepository()
    {
        // Valores base estándar según BR-COST-FORMULA-001 (176h, 335 alquiler, 100 suministros, 440 consumibles)
        var defaultMadrid = new CenterCostParameters
        {
            MonthlyOpeningHours = 176m,
            MonthlyOverheadCost = 875.00m
        };

        _centerParams["ctr-madrid-1"] = defaultMadrid;
        _centerParams["ctr-radiante-1"] = defaultMadrid;
        _centerParams["ctr-luz-belleza"] = defaultMadrid;
        _centerParams["ctr-test-free"] = defaultMadrid;
    }

    public CenterCostParameters GetParameters(string centerId)
    {
        return _centerParams.GetOrAdd(centerId, _ => new CenterCostParameters
        {
            MonthlyOpeningHours = 176m,
            MonthlyOverheadCost = 875.00m
        });
    }

    public void SaveParameters(string centerId, CenterCostParameters parameters)
    {
        _centerParams[centerId] = parameters;
    }

    public decimal GetWorkerCostPerMinute(string workerCategoryId)
    {
        if (!string.IsNullOrEmpty(workerCategoryId) && _workerRates.TryGetValue(workerCategoryId, out var rate))
            return rate;

        return 0.272727m;
    }
}
