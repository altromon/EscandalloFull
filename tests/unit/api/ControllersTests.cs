using Escandallo.Api.Controllers;
using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Escandallo.Tests.Unit.Api;

public class ControllersTests
{
    [Fact]
    public void AuthController_Login_ValidCredentials_ReturnsOkSession()
    {
        var gateway = new AuthGateway();
        var controller = new AuthController(gateway);

        var response = controller.Login(new AuthController.LoginDto("gestor@estetica.com", "SecretPass123!"));

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var session = Assert.IsType<AuthSession>(okResult.Value);
        Assert.Equal("usr-100", session.UserId);
        Assert.Equal("ctr-madrid-1", session.ActiveCenterId);
    }

    [Fact]
    public void AuthController_SelectCenter_UnauthorizedCenter_Returns403()
    {
        var gateway = new AuthGateway();
        var controller = new AuthController(gateway);

        var response = controller.SelectCenter(new AuthController.SelectCenterDto("usr-100", "ctr-barcelona-9"));

        var statusResult = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(403, statusResult.StatusCode);
    }

    [Fact]
    public void CostsController_CalculateParameters_ValidInput_ReturnsOk()
    {
        var costEngine = new EscandalloCostEngine();
        var controller = new CostsController(costEngine);

        var response = controller.CalculateParameters(new CostsController.CalculateParametersDto(176m, 335m, 100m, 440m));

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var parameters = Assert.IsType<CenterCostParameters>(okResult.Value);
        Assert.Equal(10560m, parameters.MonthlyOpeningMinutes);
        Assert.Equal(875m, parameters.MonthlyOverheadCost);
    }

    [Fact]
    public void EscandallosController_Calculate_ReturnsValidSuggestedPrices()
    {
        var costEngine = new EscandalloCostEngine();
        var marginCalc = new MarginCalculator();
        var subMgr = new SubscriptionManager();
        var repo = new EscandalloRepository();
        var controller = new EscandallosController(costEngine, marginCalc, subMgr, repo);

        var response = controller.Calculate(new EscandallosController.CalculateEscandalloDto(
            DurationMinutes: 60,
            OverheadPerMinute: 0.08285985m,
            WorkerCostPerMinute: 0.27272727m,
            TotalCosmeticsCost: 13.37723m));

        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void EscandallosController_GetAll_ReturnsServicesForCenter()
    {
        var controller = new EscandallosController(new EscandalloCostEngine(), new MarginCalculator(), new SubscriptionManager(), new EscandalloRepository());

        var response = controller.GetAll("ctr-madrid-1");

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var services = Assert.IsAssignableFrom<IEnumerable<CompleteEscandallo>>(okResult.Value);
        Assert.NotEmpty(services);
    }

    [Fact]
    public void EscandallosController_Duplicate_CreatesClonedServiceWithPrefix()
    {
        var repo = new EscandalloRepository();
        var subMgr = new SubscriptionManager();
        var controller = new EscandallosController(new EscandalloCostEngine(), new MarginCalculator(), subMgr, repo);

        var response = controller.Duplicate("esc-rad-01", "ctr-radiante-1");

        var statusResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(201, statusResult.StatusCode);
        var clone = Assert.IsType<CompleteEscandallo>(statusResult.Value);
        Assert.StartsWith("Copia de ", clone.ServiceName);
        Assert.True(clone.IsActive);
    }

    [Fact]
    public void EscandallosController_Duplicate_WhenQuotaExceeded_Returns403()
    {
        var repo = new EscandalloRepository();
        var subMgr = new SubscriptionManager();
        var controller = new EscandallosController(new EscandalloCostEngine(), new MarginCalculator(), subMgr, repo);

        // ctr-luz-belleza already has 2/2 active escandallos
        var response = controller.Duplicate("esc-luz-01", "ctr-luz-belleza");

        var statusResult = Assert.IsType<ObjectResult>(response);
        Assert.Equal(403, statusResult.StatusCode);
    }

    [Fact]
    public void EscandallosController_UpdateStatus_DeactivatingReleasesQuotaAndAllowsNewDuplicate()
    {
        var repo = new EscandalloRepository();
        var subMgr = new SubscriptionManager();
        var controller = new EscandallosController(new EscandalloCostEngine(), new MarginCalculator(), subMgr, repo);

        // 1. Deactivate esc-luz-01 in ctr-luz-belleza (was 2/2, now 1/2)
        var patchResponse = controller.UpdateStatus("esc-luz-01", new EscandallosController.UpdateEscandalloStatusDto(false), "ctr-luz-belleza");
        var okResult = Assert.IsType<OkObjectResult>(patchResponse);
        var updated = Assert.IsType<CompleteEscandallo>(okResult.Value);
        Assert.False(updated.IsActive);

        // 2. Duplicating should now succeed (was blocked at 2/2)
        var dupResponse = controller.Duplicate("esc-luz-02", "ctr-luz-belleza");
        var dupStatus = Assert.IsType<ObjectResult>(dupResponse);
        Assert.Equal(201, dupStatus.StatusCode);
    }

    [Fact]
    public void BillingController_GetStatus_ReturnsSubscriptionStatus()
    {
        var subMgr = new SubscriptionManager();
        var controller = new BillingController(subMgr);

        var response = controller.GetStatus("ctr-madrid-1");

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var status = Assert.IsType<SubscriptionStatus>(okResult.Value);
        Assert.Equal(PlanType.Free, status.Plan);
    }

    [Fact]
    public void DemoController_StatusAndPurge_ExecutesCorrectly()
    {
        var demoMgr = new DemoManager();
        var controller = new DemoController(demoMgr);

        var statusResponse = controller.GetStatus("ctr-mixed-data");
        var okStatus = Assert.IsType<OkObjectResult>(statusResponse);
        Assert.NotNull(okStatus.Value);

        var purgeResponse = controller.PurgeDemoData("ctr-mixed-data");
        var okPurge = Assert.IsType<OkObjectResult>(purgeResponse);
        Assert.NotNull(okPurge.Value);
    }

    [Fact]
    public void ExportController_ExportPdfAndXlsx_ReturnsFileStreams()
    {
        var exportEngine = new ExportDocumentEngine();
        var controller = new ExportController(exportEngine);
        var escandallos = new List<CompleteEscandallo>
        {
            new() { Id = "esc-1", ServiceName = "Facial", TotalEscandalloCost = 25m, SuggestedPvp = 50m }
        };

        var pdfResult = controller.ExportPdf("ctr-madrid-1", escandallos);
        var fileContentPdf = Assert.IsType<FileContentResult>(pdfResult);
        Assert.Equal("application/pdf", fileContentPdf.ContentType);

        var xlsxResult = controller.ExportXlsx("ctr-madrid-1", escandallos);
        var fileContentXlsx = Assert.IsType<FileContentResult>(xlsxResult);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileContentXlsx.ContentType);
    }

    [Fact]
    public void HealthController_LivenessAndReadiness_Returns200Ok()
    {
        var healthSvc = new HealthCheckService();
        var controller = new HealthController(healthSvc);

        var liveness = controller.GetLiveness();
        var okLive = Assert.IsType<OkObjectResult>(liveness);
        Assert.NotNull(okLive.Value);

        var readiness = controller.GetReadiness();
        var okReady = Assert.IsType<OkObjectResult>(readiness);
        Assert.NotNull(okReady.Value);
    }
}

