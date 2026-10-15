using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/costs")]
public class CostsController : ControllerBase
{
    private readonly IEscandalloCostEngine _costEngine;
    private readonly ICostConfigurationRepository? _costConfigRepo;

    public CostsController(
        IEscandalloCostEngine costEngine,
        ICostConfigurationRepository? costConfigRepo = null)
    {
        _costEngine = costEngine;
        _costConfigRepo = costConfigRepo;
    }

    public record CalculateParametersDto(
        decimal MonthlyOpeningHours,
        decimal LocalRent,
        decimal Supplies,
        decimal GeneralCabinConsumables);

    public record CosmeticUnitCostDto(
        decimal PackagePrice,
        decimal Capacity,
        decimal WastePercentage);

    [HttpPost("parameters")]
    public ActionResult<CenterCostParameters> CalculateParameters(
        [FromBody] CalculateParametersDto dto,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        var result = _costEngine.CalculateCostParameters(
            dto.MonthlyOpeningHours,
            dto.LocalRent,
            dto.Supplies,
            dto.GeneralCabinConsumables);

        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        _costConfigRepo?.SaveParameters(targetCenter, result);

        return Ok(result);
    }

    [HttpPost("cosmetic-unit-cost")]
    public ActionResult CalculateCosmeticUnitCost([FromBody] CosmeticUnitCostDto dto)
    {
        var (grossCost, effectiveCost) = _costEngine.CalculateCosmeticUnitCost(
            dto.PackagePrice,
            dto.Capacity,
            dto.WastePercentage);

        return Ok(new { grossUnitCost = grossCost, effectiveUnitCost = effectiveCost });
    }
}
