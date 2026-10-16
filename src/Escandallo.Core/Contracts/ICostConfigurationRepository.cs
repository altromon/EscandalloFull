namespace Escandallo.Core.Contracts;

public interface ICostConfigurationRepository
{
    CenterCostParameters GetParameters(string centerId);
    void SaveParameters(string centerId, CenterCostParameters parameters);
    decimal GetWorkerCostPerMinute(string workerCategoryId);
    IEnumerable<WorkerRate> GetWorkers(string centerId);
    WorkerRate SaveWorker(string centerId, string categoryName, decimal monthlyCompanyCost);
    WorkerRate? UpdateWorker(string centerId, string workerId, string categoryName, decimal monthlyCompanyCost);
    bool DeleteWorker(string centerId, string workerId);
    IEnumerable<CosmeticProduct> GetProducts(string centerId);
    CosmeticProduct SaveProduct(string centerId, string name, decimal packagePrice, decimal capacity, UnitOfMeasure unit, decimal wastePercentage);
    CosmeticProduct? UpdateProduct(string centerId, string productId, string name, decimal packagePrice, decimal capacity, UnitOfMeasure unit, decimal wastePercentage);
    bool UpdateProductPrice(string centerId, string productId, decimal newPackagePrice);
    bool DeleteProduct(string centerId, string productId);
}
