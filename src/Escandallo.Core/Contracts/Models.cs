namespace Escandallo.Core.Contracts;

public class AuthSession
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ActiveCenterId { get; set; } = string.Empty;
    public RoleType Role { get; set; }
}

public class CenterMembership
{
    public string CenterId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public RoleType Role { get; set; }
    public bool IsPlanPremium { get; set; }
}

public class CenterCostParameters
{
    public decimal MonthlyOpeningHours { get; set; } = 176m; // Unit: h/mes
    public decimal MonthlyOpeningMinutes => MonthlyOpeningHours * 60m; // Unit: min/mes (10,560)
    public decimal LocalRent { get; set; } = 335.00m;
    public decimal Supplies { get; set; } = 100.00m;
    public decimal GeneralCabinConsumables { get; set; } = 440.00m;
    public decimal MonthlyOverheadCost { get; set; } // Unit: EUR/mes
    public decimal OverheadCostPerMinute => MonthlyOpeningMinutes > 0 ? MonthlyOverheadCost / MonthlyOpeningMinutes : 0m;
}

public class WorkerRate
{
    public string Id { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal MonthlyCompanyCost { get; set; } // Unit: EUR/mes
    public decimal CostPerMinute { get; set; }      // Unit: EUR/min
}

public class CosmeticProduct
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal PackagePrice { get; set; }       // Unit: EUR
    public decimal Capacity { get; set; }           // In UnitOfMeasure
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public decimal WastePercentage { get; set; }    // In tanto por 100 (e.g. 5.0 for 5%)
    public decimal GrossUnitCost { get; set; }      // Unit: EUR/unit
    public decimal EffectiveUnitCost { get; set; }  // Unit: EUR/unit (with merma)
    public bool IsDemo { get; set; }
}

public class ConsumableUsage
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal AmountUsed { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public decimal ComputedCost { get; set; }
}

public class GuestConsumableUsage
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal AmountUsed { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
}

public class SessionPack
{
    public string Id { get; set; } = string.Empty;
    public string PackName { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public decimal TotalPackPrice { get; set; }                  // Unit: EUR (with VAT)
    public decimal EffectiveSessionPriceWithoutTax { get; set; } // Unit: EUR
    public decimal NetMarginPercentage { get; set; }             // Tanto por 100
    public TrafficLightColor TrafficLightColor { get; set; }
    public string? LossWarningMessage { get; set; }
}

public class GuestSessionPack
{
    public string Id { get; set; } = string.Empty;
    public string PackName { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public decimal TotalPackPrice { get; set; }
    public TrafficLightColor TrafficLightColor { get; set; }
}

public class CompleteEscandallo
{
    public string Id { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public TaxRate TaxRatePercentage { get; set; } = TaxRate.General21;
    public string WorkerCategory { get; set; } = string.Empty;
    public decimal LaborCost { get; set; }
    public decimal OverheadCost { get; set; }
    public decimal CosmeticsCost { get; set; }
    public decimal TotalEscandalloCost { get; set; }
    public decimal SuggestedBasePrice { get; set; }
    public decimal SuggestedPvp { get; set; }
    public decimal AppliedPvr { get; set; }
    public decimal NetMarginAbsolute { get; set; }
    public decimal NetMarginPercentage { get; set; }
    public TrafficLightColor TrafficLightColor { get; set; }
    public string? LossWarningMessage { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemo { get; set; }
    public List<SessionPack> Packs { get; set; } = new();
    public List<ConsumableUsage> Consumables { get; set; } = new();
}

public class GuestEscandallo
{
    public string Id { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public TrafficLightColor TrafficLightColor { get; set; }
    public bool IsActive { get; set; }
    public List<GuestSessionPack> Packs { get; set; } = new();
    public List<GuestConsumableUsage> Consumables { get; set; } = new();
}

public class SubscriptionStatus
{
    public PlanType Plan { get; set; } = PlanType.Free;
    public int ActiveEscandallos { get; set; }
    public int MaxAllowedEscandallos { get; set; } = 2;
    public bool CanCreateMore => Plan == PlanType.Premium || ActiveEscandallos < MaxAllowedEscandallos;
    public bool IsMockProvider { get; set; }
    public DateTime? RenewsAtUtc { get; set; }
}

public class AuditEntry
{
    public string Id { get; set; } = string.Empty;
    public string CenterId { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal NewValue { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string ChainedHashSha256 { get; set; } = string.Empty;
}

public class CreateEscandalloRequest
{
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public TaxRate TaxRatePercentage { get; set; } = TaxRate.General21;
    public string WorkerCategoryId { get; set; } = string.Empty;
    public decimal AppliedPvr { get; set; }
    public List<ConsumableUsage> Consumables { get; set; } = new();
    public List<SessionPack> Packs { get; set; } = new();
}

public class CreateCheckoutRequest
{
    public string PlanBillingCycle { get; set; } = "monthly"; // "monthly" or "annual"
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public class StripeWebhookEvent
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public long CreatedTimestamp { get; set; }
    public string CenterId { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
}
