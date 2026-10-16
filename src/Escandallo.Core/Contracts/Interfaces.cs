namespace Escandallo.Core.Contracts;

public interface IEscandalloCostEngine
{
    CenterCostParameters CalculateCostParameters(decimal monthlyOpeningHours, decimal localRent, decimal supplies, decimal generalCabinConsumables);
    (decimal grossUnitCost, decimal effectiveUnitCost) CalculateCosmeticUnitCost(decimal packagePrice, decimal capacity, decimal wastePercentage);
    decimal CalculateEscandallo(int durationMinutes, decimal overheadPerMinute, decimal workerCostPerMinute, decimal totalCosmeticsCost);
}

public interface IMarginCalculator
{
    (decimal suggestedBasePrice, decimal suggestedPvp) CalculateSuggestedPrices(decimal totalEscandallo, decimal markupPercentage, decimal taxRate);
    (decimal netMarginAbsolute, decimal netMarginPercentage) CalculateRealMargin(decimal appliedPvr, decimal totalEscandallo, decimal taxRate);
    (decimal effectiveSessionPrice, decimal netMarginPercentage) CalculatePackMargin(decimal totalPackPvr, int sessionCount, decimal totalEscandallo, decimal taxRate);
    TrafficLightColor DetermineTrafficLight(decimal marginPercentage);
    string? FormatLossWarning(decimal netMarginAbsolute, bool isPack = false);
}

public interface ISubscriptionManager
{
    SubscriptionStatus GetStatus(string centerId);
    bool CanCreateEscandallo(string centerId);
    void ValidateAndRegisterEscandallo(string centerId, string escandalloId);
    void DecrementActiveEscandallos(string centerId);
    void ReactivateEscandallo(string centerId, string escandalloId);
    string CreateCheckoutSession(string centerId, CreateCheckoutRequest request);
    bool ProcessWebhook(string rawPayload, string signatureHeader);
    SubscriptionStatus UpgradeToPremium(string centerId);
    SubscriptionStatus CancelSubscription(string centerId);
    void ResetActiveCount(string centerId);
}

public interface IEscandalloRepository
{
    IEnumerable<CompleteEscandallo> GetByCenter(string centerId);
    CompleteEscandallo? GetById(string centerId, string id);
    void Save(string centerId, CompleteEscandallo escandallo);
    bool Delete(string centerId, string id);
    int Clear(string centerId);
}

public interface IExportDocumentEngine
{
    byte[] ExportPdf(string centerId, IEnumerable<CompleteEscandallo> escandallos);
    byte[] ExportXlsx(string centerId, IEnumerable<CompleteEscandallo> escandallos);
    string SanitizeCell(string input);
}

public interface IAuthGateway
{
    AuthSession Login(string email, string password);
    IEnumerable<CenterMembership> GetAuthorizedCenters(string userId);
    AuthSession SelectCenter(string userId, string centerId);
    bool ValidateSession(string token, string expectedCenterId);
    AuthSession Register(string email, string password, string? centerName = null, RoleType role = RoleType.User);
    CenterMembership CreateCenter(string userId, string centerName);
}

public interface IDemoManager
{
    (bool hasDemoData, int demoCount, int realCount) GetStatus(string centerId);
    void LoadDemoData(string centerId, bool overrideIfRealExists);
    int PurgeDemoData(string centerId);
}

public interface IAuditTrailRepository
{
    void RecordEntry(AuditEntry entry);
    IEnumerable<AuditEntry> GetHistory(string centerId, string? resourceId = null);
}

public interface IHealthCheckService
{
    (string status, long responseTimeMs) CheckLiveness();
    (string status, Dictionary<string, string> componentDetails) CheckReadiness();
}
