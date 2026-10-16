using Escandallo.Core.Contracts;
using Escandallo.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/escandallos")]
public class EscandallosController : ControllerBase
{
    private readonly IEscandalloCostEngine _costEngine;
    private readonly IMarginCalculator _marginCalculator;
    private readonly ISubscriptionManager _subscriptionManager;
    private readonly IEscandalloRepository _repository;
    private readonly ICostConfigurationRepository _costConfigRepository;
    private readonly IAuditTrailRepository? _auditTrailRepo;

    public EscandallosController(
        IEscandalloCostEngine costEngine,
        IMarginCalculator marginCalculator,
        ISubscriptionManager subscriptionManager,
        IEscandalloRepository repository,
        ICostConfigurationRepository? costConfigRepository = null,
        IAuditTrailRepository? auditTrailRepo = null)
    {
        _costEngine = costEngine;
        _marginCalculator = marginCalculator;
        _subscriptionManager = subscriptionManager;
        _repository = repository;
        _costConfigRepository = costConfigRepository ?? new CostConfigurationRepository();
        _auditTrailRepo = auditTrailRepo;
    }

    public record CalculateEscandalloDto(
        int DurationMinutes,
        decimal OverheadPerMinute,
        decimal WorkerCostPerMinute,
        decimal TotalCosmeticsCost,
        decimal MarkupPercentage = 50.0m,
        decimal TaxRate = 21.0m);

    public record UpdateEscandalloStatusDto(bool IsActive);

    [HttpPost("calculate")]
    public ActionResult Calculate([FromBody] CalculateEscandalloDto dto)
    {
        decimal total = _costEngine.CalculateEscandallo(
            dto.DurationMinutes,
            dto.OverheadPerMinute,
            dto.WorkerCostPerMinute,
            dto.TotalCosmeticsCost);

        var (suggestedBase, suggestedPvp) = _marginCalculator.CalculateSuggestedPrices(
            total,
            dto.MarkupPercentage,
            dto.TaxRate);

        return Ok(new
        {
            totalEscandalloCost = total,
            suggestedBasePrice = suggestedBase,
            suggestedPvp = suggestedPvp
        });
    }

    [HttpGet]
    public ActionResult<IEnumerable<CompleteEscandallo>> GetAll([FromHeader(Name = "X-Center-Id")] string? centerId)
    {
        string targetCenter = centerId ?? "ctr-madrid-1";
        var services = _repository.GetByCenter(targetCenter);
        return Ok(services);
    }

    [HttpGet("{id}")]
    public ActionResult<CompleteEscandallo> GetById([FromRoute] string id, [FromHeader(Name = "X-Center-Id")] string? centerId)
    {
        string targetCenter = centerId ?? "ctr-madrid-1";
        var escandallo = _repository.GetById(targetCenter, id);
        if (escandallo == null)
        {
            return NotFound(new { message = $"Escandallo {id} no encontrado." });
        }
        return Ok(escandallo);
    }

    [HttpPost]
    public ActionResult Create([FromHeader(Name = "X-Center-Id")] string? centerId, [FromBody] CreateEscandalloRequest request)
    {
        string targetCenter = centerId ?? "ctr-madrid-1";
        string newId = Guid.NewGuid().ToString();
        try
        {
            _subscriptionManager.ValidateAndRegisterEscandallo(targetCenter, newId);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(403, new { message = ex.Message, code = "QUOTA_EXCEEDED" });
        }

        var escandallo = BuildEscandalloFromRequest(newId, request, targetCenter);
        _repository.Save(targetCenter, escandallo);
        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = 0,
            NewValue = escandallo.AppliedPvr,
            ChangeType = $"CREATE_SERVICE:{escandallo.ServiceName}"
        });
        return StatusCode(201, escandallo);
    }

    [HttpPost("{id}/duplicate")]
    public ActionResult Duplicate([FromRoute] string id, [FromHeader(Name = "X-Center-Id")] string? centerId)
    {
        string targetCenter = centerId ?? "ctr-madrid-1";
        var original = _repository.GetById(targetCenter, id);
        if (original == null)
        {
            return NotFound(new { message = $"Escandallo {id} no encontrado." });
        }

        string newId = Guid.NewGuid().ToString();
        try
        {
            _subscriptionManager.ValidateAndRegisterEscandallo(targetCenter, newId);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(403, new { message = ex.Message, code = "QUOTA_EXCEEDED" });
        }

        var clone = CloneService(newId, original);
        _repository.Save(targetCenter, clone);
        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = original.AppliedPvr,
            NewValue = clone.AppliedPvr,
            ChangeType = $"DUPLICATE_SERVICE:{clone.ServiceName}"
        });
        return StatusCode(201, clone);
    }

    [HttpPatch("{id}/status")]
    public ActionResult UpdateStatus([FromRoute] string id, [FromBody] UpdateEscandalloStatusDto dto, [FromHeader(Name = "X-Center-Id")] string? centerId)
    {
        string targetCenter = centerId ?? "ctr-madrid-1";
        var escandallo = _repository.GetById(targetCenter, id);
        if (escandallo == null)
        {
            return NotFound(new { message = $"Escandallo {id} no encontrado." });
        }

        if (escandallo.IsActive == dto.IsActive)
        {
            return Ok(escandallo);
        }

        if (dto.IsActive)
        {
            try
            {
                _subscriptionManager.ValidateAndRegisterEscandallo(targetCenter, id);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(403, new { message = ex.Message, code = "QUOTA_EXCEEDED" });
            }
            escandallo.IsActive = true;
        }
        else
        {
            _subscriptionManager.DecrementActiveEscandallos(targetCenter);
            escandallo.IsActive = false;
        }

        _repository.Save(targetCenter, escandallo);
        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = escandallo.IsActive ? 0 : 1,
            NewValue = escandallo.IsActive ? 1 : 0,
            ChangeType = $"TOGGLE_STATUS:{escandallo.ServiceName}"
        });
        return Ok(escandallo);
    }

    [HttpDelete("{id}")]
    public ActionResult Delete([FromRoute] string id, [FromHeader(Name = "X-Center-Id")] string? centerId)
    {
        string targetCenter = centerId ?? "ctr-madrid-1";
        var escandallo = _repository.GetById(targetCenter, id);
        if (escandallo == null)
        {
            return NotFound(new { message = $"Escandallo {id} no encontrado." });
        }

        bool deleted = _repository.Delete(targetCenter, id);
        if (!deleted)
        {
            return NotFound(new { message = $"No se pudo eliminar el escandallo {id}." });
        }

        if (escandallo.IsActive)
        {
            _subscriptionManager.DecrementActiveEscandallos(targetCenter);
        }

        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = escandallo.AppliedPvr,
            NewValue = 0,
            ChangeType = $"DELETE_SERVICE:{escandallo.ServiceName}"
        });

        return NoContent();
    }

    private CompleteEscandallo BuildEscandalloFromRequest(string newId, CreateEscandalloRequest request, string targetCenter)
    {
        var parameters = _costConfigRepository.GetParameters(targetCenter);
        decimal overheadPerMin = parameters.OverheadCostPerMinute;
        decimal workerPerMin = _costConfigRepository.GetWorkerCostPerMinute(request.WorkerCategoryId);
        decimal markupPct = CostConfigurationRepository.DefaultMarkupPercentage;

        decimal cosmeticsCost = request.Consumables.Sum(c => c.ComputedCost);
        decimal laborCost = request.DurationMinutes * workerPerMin;
        decimal overheadCost = request.DurationMinutes * overheadPerMin;
        decimal total = _costEngine.CalculateEscandallo(request.DurationMinutes, overheadPerMin, workerPerMin, cosmeticsCost);
        var (suggestedBase, suggestedPvp) = _marginCalculator.CalculateSuggestedPrices(total, markupPct, (decimal)request.TaxRatePercentage);
        var (netMarginAbs, netMarginPct) = _marginCalculator.CalculateRealMargin(request.AppliedPvr, total, (decimal)request.TaxRatePercentage);
        var color = _marginCalculator.DetermineTrafficLight(netMarginPct);
        string? lossWarning = _marginCalculator.FormatLossWarning(netMarginAbs);

        return new CompleteEscandallo
        {
            Id = newId,
            ServiceName = request.ServiceName,
            DurationMinutes = request.DurationMinutes,
            TaxRatePercentage = request.TaxRatePercentage,
            WorkerCategory = request.WorkerCategoryId,
            LaborCost = Math.Round(laborCost, 2),
            OverheadCost = Math.Round(overheadCost, 2),
            CosmeticsCost = Math.Round(cosmeticsCost, 2),
            TotalEscandalloCost = Math.Round(total, 2),
            SuggestedBasePrice = Math.Round(suggestedBase, 2),
            SuggestedPvp = Math.Round(suggestedPvp, 2),
            AppliedPvr = request.AppliedPvr,
            NetMarginAbsolute = Math.Round(netMarginAbs, 2),
            NetMarginPercentage = Math.Round(netMarginPct, 1),
            TrafficLightColor = color,
            LossWarningMessage = lossWarning,
            IsActive = true,
            Consumables = request.Consumables,
            Packs = request.Packs
        };
    }

    private static CompleteEscandallo CloneService(string newId, CompleteEscandallo original) => new()
    {
        Id = newId,
        ServiceName = $"Copia de {original.ServiceName}",
        DurationMinutes = original.DurationMinutes,
        TaxRatePercentage = original.TaxRatePercentage,
        WorkerCategory = original.WorkerCategory,
        LaborCost = original.LaborCost,
        OverheadCost = original.OverheadCost,
        CosmeticsCost = original.CosmeticsCost,
        TotalEscandalloCost = original.TotalEscandalloCost,
        SuggestedBasePrice = original.SuggestedBasePrice,
        SuggestedPvp = original.SuggestedPvp,
        AppliedPvr = original.AppliedPvr,
        NetMarginAbsolute = original.NetMarginAbsolute,
        NetMarginPercentage = original.NetMarginPercentage,
        TrafficLightColor = original.TrafficLightColor,
        LossWarningMessage = original.LossWarningMessage,
        IsActive = true,
        Consumables = original.Consumables.Select(c => new ConsumableUsage
        {
            ProductId = c.ProductId,
            ProductName = c.ProductName,
            AmountUsed = c.AmountUsed,
            UnitOfMeasure = c.UnitOfMeasure,
            ComputedCost = c.ComputedCost
        }).ToList(),
        Packs = original.Packs.Select(p => new SessionPack
        {
            Id = Guid.NewGuid().ToString(),
            PackName = p.PackName,
            SessionCount = p.SessionCount,
            TotalPackPrice = p.TotalPackPrice,
            EffectiveSessionPriceWithoutTax = p.EffectiveSessionPriceWithoutTax,
            NetMarginPercentage = p.NetMarginPercentage,
            TrafficLightColor = p.TrafficLightColor,
            LossWarningMessage = p.LossWarningMessage
        }).ToList()
    };
}
