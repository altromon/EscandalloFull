namespace Escandallo.Core.Contracts;

public interface ICostConfigurationRepository
{
    CenterCostParameters GetParameters(string centerId);
    void SaveParameters(string centerId, CenterCostParameters parameters);
    decimal GetWorkerCostPerMinute(string workerCategoryId);
}
