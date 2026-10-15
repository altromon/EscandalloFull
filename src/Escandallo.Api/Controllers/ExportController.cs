using Escandallo.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Escandallo.Api.Controllers;

[ApiController]
[Route("api/v1/export")]
public class ExportController : ControllerBase
{
    private readonly IExportDocumentEngine _exportEngine;
    private readonly IEscandalloRepository? _repository;

    public ExportController(IExportDocumentEngine exportEngine, IEscandalloRepository? repository = null)
    {
        _exportEngine = exportEngine;
        _repository = repository;
    }

    [HttpGet("pdf")]
    public IActionResult GetExportPdf([FromQuery] string? centerId = null, [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var escandallos = _repository?.GetByCenter(targetCenter) ?? Enumerable.Empty<CompleteEscandallo>();
        byte[] pdfBytes = _exportEngine.ExportPdf(targetCenter, escandallos);
        return File(pdfBytes, "application/pdf", $"escandallo_{targetCenter}.pdf");
    }

    [HttpGet("xlsx")]
    public IActionResult GetExportXlsx([FromQuery] string? centerId = null, [FromHeader(Name = "X-Center-Id")] string? headerCenterId = null)
    {
        string targetCenter = centerId ?? headerCenterId ?? "ctr-madrid-1";
        var escandallos = _repository?.GetByCenter(targetCenter) ?? Enumerable.Empty<CompleteEscandallo>();
        byte[] xlsxBytes = _exportEngine.ExportXlsx(targetCenter, escandallos);
        return File(xlsxBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"escandallo_{targetCenter}.xlsx");
    }

    [HttpPost("pdf/{centerId}")]
    public IActionResult ExportPdf(string centerId, [FromBody] IEnumerable<CompleteEscandallo> escandallos)
    {
        byte[] pdfBytes = _exportEngine.ExportPdf(centerId, escandallos);
        return File(pdfBytes, "application/pdf", $"escandallo_{centerId}.pdf");
    }

    [HttpPost("xlsx/{centerId}")]
    public IActionResult ExportXlsx(string centerId, [FromBody] IEnumerable<CompleteEscandallo> escandallos)
    {
        byte[] xlsxBytes = _exportEngine.ExportXlsx(centerId, escandallos);
        return File(xlsxBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"escandallo_{centerId}.xlsx");
    }
}
