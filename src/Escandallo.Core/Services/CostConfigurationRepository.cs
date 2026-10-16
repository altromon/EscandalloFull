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
    private readonly ConcurrentDictionary<string, List<WorkerRate>> _workersByCenter = new();
    private readonly ConcurrentDictionary<string, List<CosmeticProduct>> _productsByCenter = new();
    private readonly object _lock = new();

    public CostConfigurationRepository()
    {
        // Valores base estándar según BR-COST-FORMULA-001 (176h, 335 alquiler, 100 suministros, 440 consumibles)
        var defaultCenters = new[] { "ctr-madrid-1", "ctr-radiante-1", "ctr-luz-belleza", "ctr-test-free" };
        foreach (var cid in defaultCenters)
        {
            _centerParams[cid] = new CenterCostParameters
            {
                MonthlyOpeningHours = 176m,
                LocalRent = 335.00m,
                Supplies = 100.00m,
                GeneralCabinConsumables = 440.00m,
                MonthlyOverheadCost = 875.00m
            };
            _workersByCenter[cid] = GetDefaultWorkers();
            _productsByCenter[cid] = GetDefaultProducts();
        }
    }

    private static List<WorkerRate> GetDefaultWorkers() => new()
    {
        new() { Id = "wrk-01", CategoryName = "Técnico", MonthlyCompanyCost = 1980.00m, CostPerMinute = Math.Round(1980.00m / 10560m, 6) },
        new() { Id = "wrk-02", CategoryName = "Técnico Superior", MonthlyCompanyCost = 2340.00m, CostPerMinute = Math.Round(2340.00m / 10560m, 6) },
        new() { Id = "wrk-03", CategoryName = "Encargado", MonthlyCompanyCost = 2880.00m, CostPerMinute = Math.Round(2880.00m / 10560m, 6) },
        new() { Id = "wrk-04", CategoryName = "Esteticista", MonthlyCompanyCost = 2880.00m, CostPerMinute = Math.Round(2880.00m / 10560m, 6) },
        new() { Id = "wrk-05", CategoryName = "Auxiliar", MonthlyCompanyCost = 2400.00m, CostPerMinute = Math.Round(2400.00m / 10560m, 6) },
        new() { Id = "wrk-06", CategoryName = "Enfermero", MonthlyCompanyCost = 3294.00m, CostPerMinute = Math.Round(3294.00m / 10560m, 6) },
        new() { Id = "wrk-07", CategoryName = "Doctor", MonthlyCompanyCost = 5625.00m, CostPerMinute = Math.Round(5625.00m / 10560m, 6) }
    };

    private static List<CosmeticProduct> GetDefaultProducts() => new()
    {
        new() { Id = "cos-01", Name = "Leche Limpiadora", PackagePrice = 36.00m, Capacity = 500m, UnitOfMeasure = UnitOfMeasure.Ml, WastePercentage = 0m, GrossUnitCost = 0.072m, EffectiveUnitCost = 0.072m, IsDemo = true },
        new() { Id = "cos-02", Name = "Vial Principios Activos", PackagePrice = 11.50m, Capacity = 1m, UnitOfMeasure = UnitOfMeasure.Unidad, WastePercentage = 0m, GrossUnitCost = 11.50m, EffectiveUnitCost = 11.50m, IsDemo = true },
        new() { Id = "cos-03", Name = "Crema Finalizadora", PackagePrice = 60.80m, Capacity = 200m, UnitOfMeasure = UnitOfMeasure.Ml, WastePercentage = 0m, GrossUnitCost = 0.304m, EffectiveUnitCost = 0.304m, IsDemo = true },
        new() { Id = "cos-04", Name = "Aceite Esencial Masaje", PackagePrice = 45.00m, Capacity = 250m, UnitOfMeasure = UnitOfMeasure.Ml, WastePercentage = 0m, GrossUnitCost = 0.180m, EffectiveUnitCost = 0.180m, IsDemo = true }
    };

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
        if (string.IsNullOrWhiteSpace(workerCategoryId))
            return 0.272727m;

        foreach (var list in _workersByCenter.Values)
        {
            var match = list.FirstOrDefault(w => w.CategoryName.Equals(workerCategoryId, StringComparison.OrdinalIgnoreCase) || w.Id.Equals(workerCategoryId, StringComparison.OrdinalIgnoreCase));
            if (match != null)
                return match.CostPerMinute;
        }

        return 0.272727m;
    }

    public IEnumerable<WorkerRate> GetWorkers(string centerId)
    {
        lock (_lock)
        {
            var list = _workersByCenter.GetOrAdd(centerId, _ => GetDefaultWorkers());
            return list.ToList();
        }
    }

    public WorkerRate SaveWorker(string centerId, string categoryName, decimal monthlyCompanyCost)
    {
        lock (_lock)
        {
            var list = _workersByCenter.GetOrAdd(centerId, _ => GetDefaultWorkers());
            var paramsCenter = GetParameters(centerId);
            decimal minutes = paramsCenter.MonthlyOpeningMinutes > 0 ? paramsCenter.MonthlyOpeningMinutes : 10560m;
            decimal costPerMin = Math.Round(monthlyCompanyCost / minutes, 6);

            var existing = list.FirstOrDefault(w => w.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.MonthlyCompanyCost = monthlyCompanyCost;
                existing.CostPerMinute = costPerMin;
                return existing;
            }

            var worker = new WorkerRate
            {
                Id = $"wrk-{Guid.NewGuid().ToString()[..6]}",
                CategoryName = categoryName,
                MonthlyCompanyCost = monthlyCompanyCost,
                CostPerMinute = costPerMin
            };
            list.Add(worker);
            return worker;
        }
    }

    public WorkerRate? UpdateWorker(string centerId, string workerId, string categoryName, decimal monthlyCompanyCost)
    {
        lock (_lock)
        {
            var list = _workersByCenter.GetOrAdd(centerId, _ => GetDefaultWorkers());
            var worker = list.FirstOrDefault(w => w.Id.Equals(workerId, StringComparison.OrdinalIgnoreCase));
            if (worker == null) return null;

            var paramsCenter = GetParameters(centerId);
            decimal minutes = paramsCenter.MonthlyOpeningMinutes > 0 ? paramsCenter.MonthlyOpeningMinutes : 10560m;
            decimal costPerMin = Math.Round(monthlyCompanyCost / minutes, 6);

            worker.CategoryName = categoryName;
            worker.MonthlyCompanyCost = monthlyCompanyCost;
            worker.CostPerMinute = costPerMin;
            return worker;
        }
    }

    public bool DeleteWorker(string centerId, string workerId)
    {
        lock (_lock)
        {
            if (_workersByCenter.TryGetValue(centerId, out var list))
            {
                return list.RemoveAll(w => w.Id.Equals(workerId, StringComparison.OrdinalIgnoreCase)) > 0;
            }
            return false;
        }
    }

    public IEnumerable<CosmeticProduct> GetProducts(string centerId)
    {
        lock (_lock)
        {
            var list = _productsByCenter.GetOrAdd(centerId, _ => GetDefaultProducts());
            return list.ToList();
        }
    }

    public CosmeticProduct SaveProduct(string centerId, string name, decimal packagePrice, decimal capacity, UnitOfMeasure unit, decimal wastePercentage)
    {
        lock (_lock)
        {
            var list = _productsByCenter.GetOrAdd(centerId, _ => GetDefaultProducts());
            decimal gross = capacity > 0 ? Math.Round(packagePrice / capacity, 5) : 0m;
            decimal effective = Math.Round(gross * (1m + (wastePercentage / 100m)), 5);

            var existing = list.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.PackagePrice = packagePrice;
                existing.Capacity = capacity;
                existing.UnitOfMeasure = unit;
                existing.WastePercentage = wastePercentage;
                existing.GrossUnitCost = gross;
                existing.EffectiveUnitCost = effective;
                return existing;
            }

            var product = new CosmeticProduct
            {
                Id = $"cos-{Guid.NewGuid().ToString()[..6]}",
                Name = name,
                PackagePrice = packagePrice,
                Capacity = capacity,
                UnitOfMeasure = unit,
                WastePercentage = wastePercentage,
                GrossUnitCost = gross,
                EffectiveUnitCost = effective,
                IsDemo = false
            };
            list.Add(product);
            return product;
        }
    }

    public CosmeticProduct? UpdateProduct(string centerId, string productId, string name, decimal packagePrice, decimal capacity, UnitOfMeasure unit, decimal wastePercentage)
    {
        lock (_lock)
        {
            var list = _productsByCenter.GetOrAdd(centerId, _ => GetDefaultProducts());
            var product = list.FirstOrDefault(p => p.Id.Equals(productId, StringComparison.OrdinalIgnoreCase));
            if (product == null) return null;

            decimal gross = capacity > 0 ? Math.Round(packagePrice / capacity, 5) : 0m;
            decimal effective = Math.Round(gross * (1m + (wastePercentage / 100m)), 5);

            product.Name = name;
            product.PackagePrice = packagePrice;
            product.Capacity = capacity;
            product.UnitOfMeasure = unit;
            product.WastePercentage = wastePercentage;
            product.GrossUnitCost = gross;
            product.EffectiveUnitCost = effective;
            return product;
        }
    }

    public bool UpdateProductPrice(string centerId, string productId, decimal newPackagePrice)
    {
        lock (_lock)
        {
            var list = _productsByCenter.GetOrAdd(centerId, _ => GetDefaultProducts());
            var product = list.FirstOrDefault(p => p.Id.Equals(productId, StringComparison.OrdinalIgnoreCase));
            if (product == null) return false;

            product.PackagePrice = newPackagePrice;
            product.GrossUnitCost = product.Capacity > 0 ? Math.Round(newPackagePrice / product.Capacity, 5) : 0m;
            product.EffectiveUnitCost = Math.Round(product.GrossUnitCost * (1m + (product.WastePercentage / 100m)), 5);
            return true;
        }
    }

    public bool DeleteProduct(string centerId, string productId)
    {
        lock (_lock)
        {
            if (_productsByCenter.TryGetValue(centerId, out var list))
            {
                return list.RemoveAll(p => p.Id.Equals(productId, StringComparison.OrdinalIgnoreCase)) > 0;
            }
            return false;
        }
    }
}
