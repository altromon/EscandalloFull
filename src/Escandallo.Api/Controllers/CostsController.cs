using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/costs")]
public class CostsController : ControllerBase
{
    private readonly IEscandalloCostEngine _costEngine;
    private readonly ICostConfigurationRepository? _costConfigRepo;
    private readonly IAuditTrailRepository? _auditTrailRepo;

    public CostsController(
        IEscandalloCostEngine costEngine,
        ICostConfigurationRepository? costConfigRepo = null,
        IAuditTrailRepository? auditTrailRepo = null)
    {
        _costEngine = costEngine;
        _costConfigRepo = costConfigRepo;
        _auditTrailRepo = auditTrailRepo;
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

    [HttpGet("parameters")]
    public ActionResult<CenterCostParameters> GetParameters(
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var result = _costConfigRepo?.GetParameters(targetCenter) ?? new CenterCostParameters();
        return Ok(result);
    }

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
        var prevParams = _costConfigRepo?.GetParameters(targetCenter);
        decimal prevOverhead = prevParams?.OverheadCostPerMinute ?? 0;
        _costConfigRepo?.SaveParameters(targetCenter, result);

        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = prevOverhead,
            NewValue = result.OverheadCostPerMinute,
            ChangeType = "UPDATE_SALON_COSTS:Gastos generales y estructura"
        });

        return Ok(result);
    }

    public record CreateWorkerDto(string CategoryName, decimal MonthlyCompanyCost);
    public record UpdateWorkerDto(string CategoryName, decimal MonthlyCompanyCost);
    public record CreateCosmeticDto(string Name, decimal PackagePrice, decimal Capacity, UnitOfMeasure UnitOfMeasure, decimal WastePercentage);
    public record UpdateCosmeticDto(string? Name, decimal? PackagePrice, decimal? Capacity, UnitOfMeasure? UnitOfMeasure, decimal? WastePercentage, decimal? NewPackagePrice);
    public record UpdateCosmeticPriceDto(decimal NewPackagePrice);

    [HttpGet("workers")]
    public ActionResult<IEnumerable<WorkerRate>> GetWorkers(
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var workers = _costConfigRepo?.GetWorkers(targetCenter) ?? Enumerable.Empty<WorkerRate>();
        return Ok(workers);
    }

    [HttpPost("workers")]
    public ActionResult<WorkerRate> CreateWorker(
        [FromBody] CreateWorkerDto dto,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        if (dto.MonthlyCompanyCost <= 0)
            return BadRequest(new { message = "El coste mensual debe ser mayor que cero." });

        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var worker = _costConfigRepo?.SaveWorker(targetCenter, dto.CategoryName, dto.MonthlyCompanyCost);
        if (worker != null)
        {
            _auditTrailRepo?.RecordEntry(new AuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                CenterId = targetCenter,
                ActorId = "User",
                TimestampUtc = DateTime.UtcNow,
                PreviousValue = 0,
                NewValue = worker.MonthlyCompanyCost,
                ChangeType = $"CREATE_WORKER:{worker.CategoryName}"
            });
        }
        return StatusCode(201, worker);
    }

    [HttpPut("workers/{id}")]
    public ActionResult<WorkerRate> UpdateWorker(
        [FromRoute] string id,
        [FromBody] UpdateWorkerDto dto,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        if (dto.MonthlyCompanyCost <= 0)
            return BadRequest(new { message = "El coste mensual debe ser mayor que cero." });

        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var prevWorker = _costConfigRepo?.GetWorkers(targetCenter).FirstOrDefault(w => w.Id == id);
        decimal prevCost = prevWorker?.MonthlyCompanyCost ?? 0;

        var updated = _costConfigRepo?.UpdateWorker(targetCenter, id, dto.CategoryName, dto.MonthlyCompanyCost);
        if (updated == null)
            return NotFound(new { message = $"Trabajador {id} no encontrado en el centro {targetCenter}." });

        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = prevCost,
            NewValue = updated.MonthlyCompanyCost,
            ChangeType = $"UPDATE_WORKER:{updated.CategoryName}"
        });

        return Ok(updated);
    }

    [HttpDelete("workers/{id}")]
    public ActionResult DeleteWorker(
        [FromRoute] string id,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var prevWorker = _costConfigRepo?.GetWorkers(targetCenter).FirstOrDefault(w => w.Id == id);

        bool deleted = _costConfigRepo?.DeleteWorker(targetCenter, id) ?? false;
        if (!deleted)
            return NotFound(new { message = $"Trabajador {id} no encontrado en el centro {targetCenter}." });

        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = prevWorker?.MonthlyCompanyCost ?? 0,
            NewValue = 0,
            ChangeType = $"DELETE_WORKER:{prevWorker?.CategoryName ?? id}"
        });

        return NoContent();
    }

    [HttpGet("products")]
    public ActionResult<IEnumerable<CosmeticProduct>> GetProducts(
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var products = _costConfigRepo?.GetProducts(targetCenter) ?? Enumerable.Empty<CosmeticProduct>();
        return Ok(products);
    }

    [HttpPost("products")]
    public ActionResult<CosmeticProduct> CreateProduct(
        [FromBody] CreateCosmeticDto dto,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        if (dto.PackagePrice < 0 || dto.Capacity <= 0)
            return BadRequest(new { message = "Precio o capacidad inválidos." });

        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var product = _costConfigRepo?.SaveProduct(targetCenter, dto.Name, dto.PackagePrice, dto.Capacity, dto.UnitOfMeasure, dto.WastePercentage);
        if (product != null)
        {
            _auditTrailRepo?.RecordEntry(new AuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                CenterId = targetCenter,
                ActorId = "User",
                TimestampUtc = DateTime.UtcNow,
                PreviousValue = 0,
                NewValue = product.PackagePrice,
                ChangeType = $"CREATE_COSMETIC:{product.Name}"
            });
        }
        return StatusCode(201, product);
    }

    [HttpPut("products/{id}")]
    public ActionResult UpdateProduct(
        [FromRoute] string id,
        [FromBody] UpdateCosmeticDto dto,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var prevProd = _costConfigRepo?.GetProducts(targetCenter).FirstOrDefault(p => p.Id == id);
        decimal prevPrice = prevProd?.PackagePrice ?? 0;

        if (dto.Name != null && dto.Capacity.HasValue && dto.UnitOfMeasure.HasValue)
        {
            decimal price = dto.PackagePrice ?? dto.NewPackagePrice ?? 0;
            if (price < 0 || dto.Capacity.Value <= 0)
                return BadRequest(new { message = "Precio o capacidad inválidos." });

            var updated = _costConfigRepo?.UpdateProduct(
                targetCenter, id, dto.Name, price, dto.Capacity.Value, dto.UnitOfMeasure.Value, dto.WastePercentage ?? 0);
            if (updated == null)
                return NotFound(new { message = $"Producto {id} no encontrado en el centro {targetCenter}." });

            _auditTrailRepo?.RecordEntry(new AuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                CenterId = targetCenter,
                ActorId = "User",
                TimestampUtc = DateTime.UtcNow,
                PreviousValue = prevPrice,
                NewValue = updated.PackagePrice,
                ChangeType = $"UPDATE_COSMETIC:{updated.Name}"
            });

            return Ok(updated);
        }

        decimal newPrice = dto.NewPackagePrice ?? dto.PackagePrice ?? -1;
        if (newPrice < 0)
            return BadRequest(new { message = "El precio de compra no puede ser negativo." });

        bool priceUpdated = _costConfigRepo?.UpdateProductPrice(targetCenter, id, newPrice) ?? false;
        if (!priceUpdated)
            return NotFound(new { message = $"Producto {id} no encontrado en el centro {targetCenter}." });

        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = prevPrice,
            NewValue = newPrice,
            ChangeType = $"UPDATE_COSMETIC_PRICE:{prevProd?.Name ?? id}"
        });

        return StatusCode(202, new { message = "Precio actualizado correctamente." });
    }

    [HttpPut("products/{id}/price")]
    public ActionResult UpdateProductPrice(
        [FromRoute] string id,
        [FromBody] UpdateCosmeticPriceDto dto,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        return UpdateProduct(id, new UpdateCosmeticDto(null, null, null, null, null, dto.NewPackagePrice), headerCenterId, centerId);
    }

    [HttpDelete("products/{id}")]
    public ActionResult DeleteProduct(
        [FromRoute] string id,
        [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null,
        [FromQuery] string? centerId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var prevProd = _costConfigRepo?.GetProducts(targetCenter).FirstOrDefault(p => p.Id == id);

        bool deleted = _costConfigRepo?.DeleteProduct(targetCenter, id) ?? false;
        if (!deleted)
            return NotFound(new { message = $"Producto {id} no encontrado en el centro {targetCenter}." });

        _auditTrailRepo?.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = targetCenter,
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = prevProd?.PackagePrice ?? 0,
            NewValue = 0,
            ChangeType = $"DELETE_COSMETIC:{prevProd?.Name ?? id}"
        });

        return NoContent();
    }
}
