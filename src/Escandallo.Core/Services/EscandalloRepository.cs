using System.Collections.Concurrent;
using Escandallo.Core.Contracts;

namespace Escandallo.Core.Services;

/// <summary>
/// Repositorio en memoria thread-safe de escandallos por centro (CMP-DATA-ACCESS-001, FR-SERVICE-MARGIN-001).
/// </summary>
public class EscandalloRepository : IEscandalloRepository
{
    private readonly ConcurrentDictionary<string, List<CompleteEscandallo>> _servicesByCenter = new();
    private readonly object _lock = new();

    public EscandalloRepository()
    {
        _servicesByCenter["ctr-madrid-1"] = new List<CompleteEscandallo>
        {
            CreateDefaultMesodermolifting("esc-meso-01")
        };

        _servicesByCenter["ctr-radiante-1"] = new List<CompleteEscandallo>
        {
            CreateDefaultMesodermolifting("esc-rad-01")
        };

        _servicesByCenter["ctr-luz-belleza"] = new List<CompleteEscandallo>
        {
            CreateDefaultMesodermolifting("esc-luz-01"),
            CreateSecondaryFacial("esc-luz-02")
        };

        _servicesByCenter["ctr-test-free"] = new List<CompleteEscandallo>
        {
            CreateDefaultMesodermolifting("esc-test-01"),
            CreateSecondaryFacial("esc-test-02")
        };
    }

    internal static CompleteEscandallo CreateDefaultMesodermolifting(string id) => new()
    {
        Id = id,
        ServiceName = "Mesodermolifting",
        IsDemo = true,
        DurationMinutes = 60,
        TaxRatePercentage = TaxRate.General21,
        WorkerCategory = "Encargado",
        LaborCost = 16.36m,
        OverheadCost = 4.97m,
        CosmeticsCost = 13.38m,
        TotalEscandalloCost = 34.71m,
        SuggestedBasePrice = 52.07m,
        SuggestedPvp = 63.00m,
        AppliedPvr = 63.00m,
        NetMarginAbsolute = 17.36m,
        NetMarginPercentage = 33.3m,
        TrafficLightColor = TrafficLightColor.Amarillo,
        IsActive = true,
        Packs = new List<SessionPack> { CreateDefaultPack() },
        Consumables = CreateDefaultConsumables()
    };

    private static SessionPack CreateDefaultPack() => new()
    {
        Id = "pk-01",
        PackName = "Bono 5 sesiones",
        SessionCount = 5,
        TotalPackPrice = 300.00m,
        EffectiveSessionPriceWithoutTax = 49.59m,
        NetMarginPercentage = 30.0m,
        TrafficLightColor = TrafficLightColor.Amarillo
    };

    private static List<ConsumableUsage> CreateDefaultConsumables() => new()
    {
        new() { ProductId = "cos-01", ProductName = "Leche Limpiadora", AmountUsed = 5, UnitOfMeasure = UnitOfMeasure.Ml, ComputedCost = 0.36m },
        new() { ProductId = "cos-02", ProductName = "Vial Principios Activos", AmountUsed = 1, UnitOfMeasure = UnitOfMeasure.Unidad, ComputedCost = 11.50m },
        new() { ProductId = "cos-03", ProductName = "Crema Finalizadora", AmountUsed = 5, UnitOfMeasure = UnitOfMeasure.Ml, ComputedCost = 1.52m }
    };

    internal static CompleteEscandallo CreateSecondaryFacial(string id) => new()
    {
        Id = id,
        ServiceName = "Higiene Facial Profunda",
        IsDemo = true,
        DurationMinutes = 50,
        TaxRatePercentage = TaxRate.General21,
        WorkerCategory = "Esteticista",
        LaborCost = 13.64m,
        OverheadCost = 4.14m,
        CosmeticsCost = 8.50m,
        TotalEscandalloCost = 26.28m,
        SuggestedBasePrice = 39.42m,
        SuggestedPvp = 47.70m,
        AppliedPvr = 48.00m,
        NetMarginAbsolute = 13.39m,
        NetMarginPercentage = 33.7m,
        TrafficLightColor = TrafficLightColor.Amarillo,
        IsActive = true
    };

    public IEnumerable<CompleteEscandallo> GetByCenter(string centerId)
    {
        lock (_lock)
        {
            var list = _servicesByCenter.GetOrAdd(centerId, _ => new List<CompleteEscandallo>());
            return list.ToList();
        }
    }

    public CompleteEscandallo? GetById(string centerId, string id)
    {
        lock (_lock)
        {
            if (_servicesByCenter.TryGetValue(centerId, out var list))
            {
                return list.FirstOrDefault(x => x.Id == id);
            }
            return null;
        }
    }

    public void Save(string centerId, CompleteEscandallo escandallo)
    {
        lock (_lock)
        {
            var list = _servicesByCenter.GetOrAdd(centerId, _ => new List<CompleteEscandallo>());
            var existingIdx = list.FindIndex(x => x.Id == escandallo.Id);
            if (existingIdx >= 0)
            {
                list[existingIdx] = escandallo;
            }
            else
            {
                list.Add(escandallo);
            }
        }
    }

    public bool Delete(string centerId, string id)
    {
        lock (_lock)
        {
            if (_servicesByCenter.TryGetValue(centerId, out var list))
            {
                return list.RemoveAll(x => x.Id == id) > 0;
            }
            return false;
        }
    }

    public int Clear(string centerId)
    {
        lock (_lock)
        {
            if (_servicesByCenter.TryGetValue(centerId, out var list))
            {
                int count = list.Count;
                list.Clear();
                return count;
            }
            return 0;
        }
    }
}
