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
    public void CostsController_WorkersAndProducts_Crud_ExecutesCorrectly()
    {
        var costEngine = new EscandalloCostEngine();
        var repo = new CostConfigurationRepository();
        var controller = new CostsController(costEngine, repo);

        // 1. Get initial workers
        var workersResult = controller.GetWorkers("ctr-madrid-1");
        var okWorkers = Assert.IsType<OkObjectResult>(workersResult.Result);
        var workers = Assert.IsAssignableFrom<IEnumerable<WorkerRate>>(okWorkers.Value);
        Assert.NotEmpty(workers);

        // 2. Create worker
        var createWorkerResult = controller.CreateWorker(new CostsController.CreateWorkerDto("Fisioterapeuta", 2100m), "ctr-madrid-1");
        var statusWorker = Assert.IsType<ObjectResult>(createWorkerResult.Result);
        Assert.Equal(201, statusWorker.StatusCode);
        var createdWorker = Assert.IsType<WorkerRate>(statusWorker.Value);
        Assert.Equal("Fisioterapeuta", createdWorker.CategoryName);

        // 3. Get products
        var productsResult = controller.GetProducts("ctr-madrid-1");
        var okProducts = Assert.IsType<OkObjectResult>(productsResult.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<CosmeticProduct>>(okProducts.Value);
        Assert.NotEmpty(products);

        // 4. Create product
        var createProductResult = controller.CreateProduct(new CostsController.CreateCosmeticDto("Ampolla Vitamina C", 30m, 10m, Escandallo.Core.Contracts.UnitOfMeasure.Unidad, 0m), "ctr-madrid-1");
        var statusProduct = Assert.IsType<ObjectResult>(createProductResult.Result);
        Assert.Equal(201, statusProduct.StatusCode);
        var createdProd = Assert.IsType<CosmeticProduct>(statusProduct.Value);
        Assert.Equal("Ampolla Vitamina C", createdProd.Name);

        // 5. Update product price
        var updateResult = controller.UpdateProductPrice(createdProd.Id, new CostsController.UpdateCosmeticPriceDto(35m), "ctr-madrid-1");
        var statusUpdate = Assert.IsType<ObjectResult>(updateResult);
        Assert.Equal(202, statusUpdate.StatusCode);
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
    public void CostsController_FullCrud_Operations_Succeed()
    {
        var costEngine = new EscandalloCostEngine();
        var repo = new CostConfigurationRepository();
        var controller = new CostsController(costEngine, repo);

        // 1. Get Parameters
        var paramsRes = controller.GetParameters("ctr-madrid-1");
        var okParams = Assert.IsType<OkObjectResult>(paramsRes.Result);
        var p = Assert.IsType<CenterCostParameters>(okParams.Value);
        Assert.True(p.MonthlyOpeningHours > 0);

        // 2. Worker CRUD: Create, Update, Delete
        var createW = controller.CreateWorker(new CostsController.CreateWorkerDto("Quiromasajista", 2000m), "ctr-madrid-1");
        var w = Assert.IsType<WorkerRate>(Assert.IsType<ObjectResult>(createW.Result).Value);

        var updateW = controller.UpdateWorker(w.Id, new CostsController.UpdateWorkerDto("Quiromasajista Senior", 2400m), "ctr-madrid-1");
        var wUpdated = Assert.IsType<WorkerRate>(Assert.IsType<OkObjectResult>(updateW.Result).Value);
        Assert.Equal("Quiromasajista Senior", wUpdated.CategoryName);
        Assert.Equal(2400m, wUpdated.MonthlyCompanyCost);

        var deleteW = controller.DeleteWorker(w.Id, "ctr-madrid-1");
        Assert.IsType<NoContentResult>(deleteW);

        // 3. Product CRUD: Create, Full Update, Delete
        var createP = controller.CreateProduct(new CostsController.CreateCosmeticDto("Serum Retinol", 50m, 100m, Escandallo.Core.Contracts.UnitOfMeasure.Ml, 5m), "ctr-madrid-1");
        var prod = Assert.IsType<CosmeticProduct>(Assert.IsType<ObjectResult>(createP.Result).Value);

        var updateP = controller.UpdateProduct(prod.Id, new CostsController.UpdateCosmeticDto("Serum Retinol Puro", 60m, 120m, Escandallo.Core.Contracts.UnitOfMeasure.Ml, 6m, null), "ctr-madrid-1");
        var okProdResult = Assert.IsType<OkObjectResult>(updateP);
        var prodUpdated = Assert.IsType<CosmeticProduct>(okProdResult.Value);
        Assert.Equal("Serum Retinol Puro", prodUpdated.Name);
        Assert.Equal(60m, prodUpdated.PackagePrice);

        var deleteP = controller.DeleteProduct(prod.Id, "ctr-madrid-1");
        Assert.IsType<NoContentResult>(deleteP);
    }

    [Fact]
    public void EscandallosController_Delete_RemovesServiceAndDecrementsQuota()
    {
        var costEngine = new EscandalloCostEngine();
        var marginCalc = new MarginCalculator();
        var subMgr = new SubscriptionManager();
        var repo = new EscandalloRepository();
        var controller = new EscandallosController(costEngine, marginCalc, subMgr, repo);

        // Create an escandallo
        var createRes = controller.Create("ctr-madrid-1", new CreateEscandalloRequest
        {
            ServiceName = "Servicio Temporal Para Borrar",
            DurationMinutes = 30,
            AppliedPvr = 40m,
            WorkerCategoryId = "Esteticista"
        });
        var created = Assert.IsType<CompleteEscandallo>(Assert.IsType<ObjectResult>(createRes).Value);
        int initialActive = subMgr.GetStatus("ctr-madrid-1").ActiveEscandallos;

        // Delete the escandallo
        var deleteRes = controller.Delete(created.Id, "ctr-madrid-1");
        Assert.IsType<NoContentResult>(deleteRes);

        // Verify it is gone and quota decremented
        var getRes = controller.GetById(created.Id, "ctr-madrid-1");
        Assert.IsType<NotFoundObjectResult>(getRes.Result);
        Assert.Equal(initialActive - 1, subMgr.GetStatus("ctr-madrid-1").ActiveEscandallos);
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

    [Fact]
    public void DemoController_Purge_RemovesServicesAndResetsQuota()
    {
        var repo = new EscandalloRepository();
        var subMgr = new SubscriptionManager();
        var demoMgr = new DemoManager(repo, subMgr);
        var controller = new DemoController(demoMgr);

        // Load demo data
        controller.LoadDemoData("ctr-madrid-1", true);
        var initialServices = repo.GetByCenter("ctr-madrid-1").ToList();
        Assert.True(initialServices.Count >= 2);
        Assert.Equal(2, subMgr.GetStatus("ctr-madrid-1").ActiveEscandallos);

        // Purge demo data
        var purgeRes = controller.PurgeDemoData("ctr-madrid-1");
        var okPurge = Assert.IsType<OkObjectResult>(purgeRes);
        Assert.NotNull(okPurge.Value);

        // Verify repository is empty and active count reset to 0
        var remainingServices = repo.GetByCenter("ctr-madrid-1").ToList();
        Assert.Empty(remainingServices);
        Assert.Equal(0, subMgr.GetStatus("ctr-madrid-1").ActiveEscandallos);
    }

    [Fact]
    public void BillingController_MockUpgradeAndCancel_ModifiesPlanCorrectly()
    {
        var subMgr = new SubscriptionManager();
        var controller = new BillingController(subMgr);

        // 1. Upgrade to Pro
        var upgradeRes = controller.MockCompleteCheckout("ctr-madrid-1");
        var okUpgrade = Assert.IsType<OkObjectResult>(upgradeRes.Result);
        var statusUpgraded = Assert.IsType<SubscriptionStatus>(okUpgrade.Value);
        Assert.Equal(PlanType.Premium, statusUpgraded.Plan);
        Assert.Equal(int.MaxValue, statusUpgraded.MaxAllowedEscandallos);

        // 2. Cancel Pro subscription
        var cancelRes = controller.MockCancelSubscription("ctr-madrid-1");
        var okCancel = Assert.IsType<OkObjectResult>(cancelRes.Result);
        var statusCancelled = Assert.IsType<SubscriptionStatus>(okCancel.Value);
        Assert.Equal(PlanType.Free, statusCancelled.Plan);
        Assert.Equal(2, statusCancelled.MaxAllowedEscandallos);
    }

    [Fact]
    public void AuthController_RegisterAndCreateCenter_Succeeds()
    {
        var authGateway = new AuthGateway();
        var controller = new AuthController(authGateway);

        // 1. Register new user
        var regDto = new AuthController.RegisterDto("nuevo@estetica.com", "Password123!", "Centro Nuevo");
        var regRes = controller.Register(regDto);
        var okReg = Assert.IsType<OkObjectResult>(regRes.Result);
        var session = Assert.IsType<AuthSession>(okReg.Value);
        Assert.NotNull(session.Token);
        Assert.NotEmpty(session.UserId);

        // 2. Create another center for that user
        var createCenterDto = new AuthController.CreateCenterDto(session.UserId, "Segundo Salón Belleza");
        var centerRes = controller.CreateCenter(createCenterDto);
        var okCenter = Assert.IsType<OkObjectResult>(centerRes.Result);
        var center = Assert.IsType<CenterMembership>(okCenter.Value);
        Assert.Equal("Segundo Salón Belleza", center.Name);

        // 3. User now has 2 authorized centers
        var listRes = controller.GetAuthorizedCenters(session.UserId);
        var okList = Assert.IsType<OkObjectResult>(listRes.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<CenterMembership>>(okList.Value).ToList();
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void AuditController_GetHistory_ReturnsChainedEntries()
    {
        var auditRepo = new AuditTrailRepository();
        auditRepo.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = "ctr-test-1",
            ActorId = "user-1",
            ChangeType = "ESCANDALLO_CREATED",
            PreviousValue = 0,
            NewValue = 50,
            TimestampUtc = DateTime.UtcNow
        });
        auditRepo.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = "ctr-test-1",
            ActorId = "user-1",
            ChangeType = "ESCANDALLO_DELETED",
            PreviousValue = 50,
            NewValue = 0,
            TimestampUtc = DateTime.UtcNow
        });

        var controller = new AuditController(auditRepo);
        var result = controller.GetHistory(centerId: "ctr-test-1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var entries = Assert.IsAssignableFrom<IEnumerable<AuditEntry>>(okResult.Value).ToList();
        Assert.Equal(2, entries.Count);
        Assert.Equal("ESCANDALLO_CREATED", entries[0].ChangeType);
        Assert.Equal("ESCANDALLO_DELETED", entries[1].ChangeType);
        Assert.NotEmpty(entries[0].ChainedHashSha256);
        Assert.NotEmpty(entries[1].ChainedHashSha256);
    }

    [Fact]
    public void SystemController_GetLogs_ReturnsBufferedLogs()
    {
        var logBuffer = new SystemLogBuffer();
        logBuffer.Enqueue(new LogMessageEntry(DateTime.UtcNow, "INFO", "SystemControllerTests", "Test Log Message", "trace-123"));

        var controller = new SystemController(logBuffer);
        var result = controller.GetLogs(limit: 10);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var logs = Assert.IsAssignableFrom<IEnumerable<LogMessageEntry>>(okResult.Value).ToList();
        Assert.Single(logs);
        Assert.Equal("Test Log Message", logs[0].Message);
        Assert.Equal("trace-123", logs[0].TraceId);
    }

    [Fact]
    public void EscandallosController_RecordsAuditTrail_OnDeleteAndToggle()
    {
        var costEngine = new EscandalloCostEngine();
        var marginCalc = new MarginCalculator();
        var subMgr = new SubscriptionManager();
        var repo = new EscandalloRepository();
        var auditRepo = new AuditTrailRepository();
        var controller = new EscandallosController(costEngine, marginCalc, subMgr, repo, auditTrailRepo: auditRepo);

        // Create an escandallo
        var createRes = controller.Create("ctr-madrid-1", new CreateEscandalloRequest
        {
            ServiceName = "Tratamiento Audit",
            DurationMinutes = 45,
            AppliedPvr = 55m,
            WorkerCategoryId = "Esteticista"
        });
        var created = Assert.IsType<CompleteEscandallo>(Assert.IsType<ObjectResult>(createRes).Value);

        // Toggle status
        controller.UpdateStatus(created.Id, new EscandallosController.UpdateEscandalloStatusDto(false), "ctr-madrid-1");

        // Delete service
        controller.Delete(created.Id, "ctr-madrid-1");

        var history = auditRepo.GetHistory("ctr-madrid-1").ToList();
        Assert.True(history.Count >= 3);
        Assert.Contains(history, h => h.ChangeType.StartsWith("CREATE_SERVICE:"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("TOGGLE_STATUS:"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("DELETE_SERVICE:"));
    }

    [Fact]
    public void CostsController_RecordsAuditTrail_OnParametersWorkersAndProducts()
    {
        var costEngine = new EscandalloCostEngine();
        var configRepo = new CostConfigurationRepository();
        var auditRepo = new AuditTrailRepository();
        var controller = new CostsController(costEngine, configRepo, auditRepo);

        // 1. Calculate and save parameters
        controller.CalculateParameters(new CostsController.CalculateParametersDto(176, 500, 200, 300), "ctr-madrid-1");

        // 2. Create worker
        var createWorkerRes = controller.CreateWorker(new CostsController.CreateWorkerDto("Especialista Laser", 2500m), "ctr-madrid-1");
        var worker = Assert.IsType<WorkerRate>(Assert.IsType<ObjectResult>(createWorkerRes.Result).Value);

        // 3. Update worker
        controller.UpdateWorker(worker.Id, new CostsController.UpdateWorkerDto("Especialista Laser Pro", 2800m), "ctr-madrid-1");

        // 4. Create product
        var createProdRes = controller.CreateProduct(new CostsController.CreateCosmeticDto("Serum Acido Hialuronico", 45m, 100m, UnitOfMeasure.Ml, 5m), "ctr-madrid-1");
        var product = Assert.IsType<CosmeticProduct>(Assert.IsType<ObjectResult>(createProdRes.Result).Value);

        // 5. Update product price
        controller.UpdateProductPrice(product.Id, new CostsController.UpdateCosmeticPriceDto(52m), "ctr-madrid-1");

        // 6. Delete product and worker
        controller.DeleteProduct(product.Id, "ctr-madrid-1");
        controller.DeleteWorker(worker.Id, "ctr-madrid-1");

        var history = auditRepo.GetHistory("ctr-madrid-1").ToList();
        Assert.Contains(history, h => h.ChangeType.StartsWith("UPDATE_SALON_COSTS:"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("CREATE_WORKER:Especialista Laser"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("UPDATE_WORKER:Especialista Laser Pro"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("CREATE_COSMETIC:Serum Acido Hialuronico"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("UPDATE_COSMETIC_PRICE:Serum Acido Hialuronico"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("DELETE_COSMETIC:Serum Acido Hialuronico"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("DELETE_WORKER:Especialista Laser Pro"));
    }

    [Fact]
    public void BillingController_RecordsAuditTrail_OnUpgradeAndCancel()
    {
        var subMgr = new SubscriptionManager();
        var auditRepo = new AuditTrailRepository();
        var controller = new BillingController(subMgr, auditRepo);

        controller.MockCompleteCheckout("ctr-madrid-1");
        controller.MockCancelSubscription("ctr-madrid-1");

        var history = auditRepo.GetHistory("ctr-madrid-1").ToList();
        Assert.Contains(history, h => h.ChangeType.StartsWith("UPGRADE_PLAN_PRO:"));
        Assert.Contains(history, h => h.ChangeType.StartsWith("CANCEL_PLAN_PRO:"));
    }

    [Fact]
    public void AuditTrailRepository_ReturnsGenesis_And_SupportsAllCenters()
    {
        var auditRepo = new AuditTrailRepository();

        // Fresh center has genesis
        var historyFresh = auditRepo.GetHistory("ctr-valencia-99").ToList();
        Assert.Single(historyFresh);
        Assert.StartsWith("GENESIS:", historyFresh[0].ChangeType);
        Assert.False(string.IsNullOrEmpty(historyFresh[0].ChainedHashSha256));

        // Center with records does not duplicate genesis
        auditRepo.RecordEntry(new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = "ctr-valencia-99",
            ActorId = "User",
            TimestampUtc = DateTime.UtcNow,
            PreviousValue = 10,
            NewValue = 20,
            ChangeType = "TEST_CHANGE"
        });

        var updatedHistory = auditRepo.GetHistory("ctr-valencia-99").ToList();
        Assert.Equal(2, updatedHistory.Count);

        // "all" queries across all centers
        var allHistory = auditRepo.GetHistory("all").ToList();
        Assert.True(allHistory.Count >= 2);
    }
}

