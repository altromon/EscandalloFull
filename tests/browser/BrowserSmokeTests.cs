using Microsoft.Playwright;
using Xunit;

namespace Escandallo.Tests.Browser;

[Collection("BrowserSmokeTests")]
public class BrowserSmokeTests : IClassFixture<AppHostFixture>, IAsyncLifetime
{
    private readonly AppHostFixture _fixture;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    public BrowserSmokeTests(AppHostFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    private async Task LoginAsGestorAsync()
    {
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

        var loginModal = _page.Locator("#loginModal");
        if (await loginModal.IsVisibleAsync())
        {
            await _page.FillAsync("#loginEmail", "gestor@estetica.com");
            await _page.FillAsync("#loginPassword", "SecretPass123!");
            await _page.ClickAsync("#btnLoginSubmit");
            await Assertions.Expect(loginModal).ToBeHiddenAsync();
        }

        var badge = _page.Locator("#activeCenterBadgeText");
        await badge.WaitForAsync();

        // Restore demo seed if a prior test emptied the center so nominal tests find services
        var emptyState = _page.Locator("#stateEmpty");
        if (await emptyState.IsVisibleAsync())
        {
            var btnLoad = _page.Locator("button:has-text('🚀 Cargar datos Demo')");
            if (await btnLoad.IsVisibleAsync())
            {
                await btnLoad.ClickAsync();
                await _page.Locator("#servicesTableBody tr").First.WaitForAsync();
            }
        }
    }

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_UnauthenticatedUser_IsPresentedWithLoginModal_CannotAccessData()
    {
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

        // Login modal must be visible on initial unauthenticated visit
        var loginModal = _page.Locator("#loginModal");
        await Assertions.Expect(loginModal).ToBeVisibleAsync();

        // Logout button and user badge must be hidden
        var btnLogout = _page.Locator("#btnLogout");
        await Assertions.Expect(btnLogout).ToBeHiddenAsync();

        // Invalid credentials display error message
        await _page.FillAsync("#loginEmail", "invalid@user.com");
        await _page.FillAsync("#loginPassword", "WrongPass!");
        await _page.ClickAsync("#btnLoginSubmit");

        var loginError = _page.Locator("#loginError");
        await Assertions.Expect(loginError).ToBeVisibleAsync();
    }

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_NavigateToHome_DisplaysNavbarAndLoadsServices()
    {
        await LoginAsGestorAsync();

        // Verify Title and Navbar Badge
        var title = await _page.TitleAsync();
        Assert.Contains("Escandallo Estética", title);

        var badge = _page.Locator("#activeCenterBadgeText");
        await badge.WaitForAsync();
        var badgeText = await badge.InnerTextAsync();
        Assert.Contains("Centro Belleza Madrid", badgeText);

        // Verify Services table rows load via API
        var firstRow = _page.Locator("#servicesTableBody tr").First;
        await firstRow.WaitForAsync();
        var rowCount = await _page.Locator("#servicesTableBody tr").CountAsync();
        Assert.True(rowCount >= 1, "At least one service should load in the table");

        // Verify Quota indicator
        var quotaBadge = _page.Locator("#quotaBadge");
        Assert.True(await quotaBadge.IsVisibleAsync());
    }

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_ChangeCenter_UpdatesBadgeAndServices()
    {
        await LoginAsGestorAsync();

        // Wait for initial load
        await _page.Locator("#servicesTableBody tr").First.WaitForAsync();

        var centerSelect = _page.Locator("#centerSelect");
        await centerSelect.WaitForAsync();

        // Switch to Luz y Belleza
        await centerSelect.SelectOptionAsync("ctr-luz-belleza");

        // Badge must update
        var badge = _page.Locator("#activeCenterBadgeText");
        await Assertions.Expect(badge).ToContainTextAsync("Luz y Belleza");

        // Table must update and contain Luz y Belleza's specific treatment
        var table = _page.Locator("#servicesTableBody");
        await Assertions.Expect(table).ToContainTextAsync("Higiene Facial");
    }

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_FreemiumUpgradeModal_OpensOnQuotaExceeded()
    {
        await LoginAsGestorAsync();

        // Wait for initial load
        await _page.Locator("#servicesTableBody tr").First.WaitForAsync();

        var centerSelect = _page.Locator("#centerSelect");
        await centerSelect.WaitForAsync();

        // Switch to Luz y Belleza (2/2 capacity in Free)
        await centerSelect.SelectOptionAsync("ctr-luz-belleza");

        var quotaBadge = _page.Locator("#quotaBadge");
        await Assertions.Expect(quotaBadge).ToContainTextAsync("2/2");

        // Attempt to create new escandallo
        var btnCreate = _page.Locator("button:has-text('➕ Nuevo Escandallo')");
        await btnCreate.ClickAsync();

        // Upgrade modal must become visible
        var upgradeModal = _page.Locator("#upgradeModal");
        await Assertions.Expect(upgradeModal).ToBeVisibleAsync();
        await Assertions.Expect(upgradeModal).ToContainTextAsync("Límite de Cuota Freemium Alcanzado");

        // Close modal
        var btnClose = _page.Locator("#upgradeModal button:has-text('Cerrar')");
        await btnClose.ClickAsync();
        await Assertions.Expect(upgradeModal).ToBeHiddenAsync();
    }

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_CreateService_AddsNewRowToTable()
    {
        await LoginAsGestorAsync();

        // Ensure we are in Madrid
        var centerSelect = _page.Locator("#centerSelect");
        await centerSelect.SelectOptionAsync("ctr-madrid-1");

        // Ensure quota is available: if full (2/2), pause one service to free quota
        var quotaText = await _page.Locator("#quotaBadge").InnerTextAsync();
        if (quotaText.Contains("2/2"))
        {
            var btnPause = _page.Locator("#servicesTableBody tr button:has-text('⏸️ Pausar')").First;
            if (await btnPause.IsVisibleAsync())
            {
                await btnPause.ClickAsync();
                await Assertions.Expect(_page.Locator("#quotaBadge")).ToContainTextAsync("1/2");
            }
        }

        // Click create new
        var btnCreate = _page.Locator("button:has-text('➕ Nuevo Escandallo')");
        await btnCreate.ClickAsync();

        var createModal = _page.Locator("#createModal");
        await Assertions.Expect(createModal).ToBeVisibleAsync();

        string uniqueServiceName = $"Peeling Test {DateTime.UtcNow.Ticks % 10000}";
        await _page.FillAsync("#createServiceName", uniqueServiceName);
        await _page.FillAsync("#createDuration", "50");
        await _page.FillAsync("#createPvr", "60.00");

        var submitBtn = _page.Locator("#createModal button[type='submit']");
        await submitBtn.ClickAsync();

        // Wait for modal to close
        await Assertions.Expect(createModal).ToBeHiddenAsync();

        // Check that new row is visible in table
        var table = _page.Locator("#servicesTableBody");
        await Assertions.Expect(table).ToContainTextAsync(uniqueServiceName);
    }

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_RowActions_DuplicateAndToggleService()
    {
        await LoginAsGestorAsync();

        // Switch to ctr-radiante-1 (starts with 1 service and 1/2 quota)
        var centerSelect = _page.Locator("#centerSelect");
        await centerSelect.SelectOptionAsync("ctr-radiante-1");

        // Wait for Radiante to load
        var badge = _page.Locator("#activeCenterBadgeText");
        await Assertions.Expect(badge).ToContainTextAsync("Centro Radiante");

        // Ensure at least one service exists
        if (await _page.Locator("#stateEmpty").IsVisibleAsync())
        {
            await _page.ClickAsync("button:has-text('🚀 Cargar datos Demo')");
        }
        await _page.Locator("#servicesTableBody tr").First.WaitForAsync();

        // Click duplicate button on first row
        var btnDuplicate = _page.Locator("#servicesTableBody tr button:has-text('📋 Duplicar')").First;
        await btnDuplicate.ClickAsync();

        // Table should now have "Copia de Mesodermolifting"
        var table = _page.Locator("#servicesTableBody");
        await Assertions.Expect(table).ToContainTextAsync("Copia de Mesodermolifting");

        // Find row for the duplicate and toggle status to pause it
        var duplicateRow = _page.Locator("#servicesTableBody tr", new() { HasText = "Copia de Mesodermolifting" });
        var btnToggle = duplicateRow.Locator("button:has-text('⏸️ Pausar')");
        await btnToggle.ClickAsync();

        // Wait for status to become Inactivo and button to become Activar
        await Assertions.Expect(duplicateRow).ToContainTextAsync("Inactivo");
        await Assertions.Expect(duplicateRow.Locator("button:has-text('▶️ Activar')")).ToBeVisibleAsync();
    }

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_SaveSalonCosts_UpdatesCalculatedOverhead()
    {
        await LoginAsGestorAsync();

        // Auto-accept alert dialogs
        _page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

        // Fill inputs: 160h, rent 400, supplies 120, cabin 480 => overhead = 1000, min = 9600 => ~0.10417 €/min
        await _page.FillAsync("#inputHours", "160");
        await _page.FillAsync("#inputRent", "400.00");
        await _page.FillAsync("#inputSupplies", "120.00");
        await _page.FillAsync("#inputCabinConsumables", "480.00");

        var btnSave = _page.Locator("#costsForm button[type='submit']");
        await btnSave.ClickAsync();

        // Overhead indicator must update
        var overheadDisplay = _page.Locator("#calculatedOverheadMinute");
        await Assertions.Expect(overheadDisplay).ToContainTextAsync("0.10417 €/min");
    }

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_ConfigureWorkersAndProducts_ExcelParity()
    {
        Assert.NotNull(_page);
        await LoginAsGestorAsync();

        // Switch to personal tab
        await _page.ClickAsync("#tabBtnWorkers");
        var workersTab = _page.Locator("#tabContentWorkers");
        await Assertions.Expect(workersTab).ToBeVisibleAsync();

        // Verify default worker categories exist
        var workersTable = _page.Locator("#workersTableBody");
        await Assertions.Expect(workersTable).ToContainTextAsync("Esteticista");

        // Switch to cosmetics tab
        await _page.ClickAsync("#tabBtnProducts");
        var productsTab = _page.Locator("#tabContentProducts");
        await Assertions.Expect(productsTab).ToBeVisibleAsync();

        // Verify default cosmetic products exist
        var productsTable = _page.Locator("#productsTableBody");
        await Assertions.Expect(productsTable).ToContainTextAsync("Leche Limpiadora");
    }

    [Fact]
    [Trait("Category", "BrowserFeature")]
    [Trait("Case", "1_GastosCrud")]
    public async Task Playwright_IdentifiedCase1_Expenses_WorkersAndCosmetics_FullCrud()
    {
        Assert.NotNull(_page);
        await LoginAsGestorAsync();
        _page.Dialog += async (_, d) => await d.AcceptAsync();

        // 1. Personal (Workers) CRUD: Create, Edit, Delete
        await _page.ClickAsync("#tabBtnWorkers");
        var workersTab = _page.Locator("#tabContentWorkers");
        await Assertions.Expect(workersTab).ToBeVisibleAsync();

        await _page.ClickAsync("#tabContentWorkers button:has-text('➕ Añadir')");
        var addWorkerModal = _page.Locator("#addWorkerModal");
        await Assertions.Expect(addWorkerModal).ToBeVisibleAsync();

        string uniqueWorkerName = $"Fisio QA {DateTime.UtcNow.Ticks % 10000}";
        await _page.FillAsync("#workerCategoryName", uniqueWorkerName);
        await _page.FillAsync("#workerMonthlyCost", "2150.00");
        await _page.ClickAsync("#addWorkerModal button[type='submit']");
        await Assertions.Expect(addWorkerModal).ToBeHiddenAsync();

        var workerRow = _page.Locator("#workersTableBody tr", new() { HasText = uniqueWorkerName });
        await Assertions.Expect(workerRow).ToBeVisibleAsync();

        // Edit worker
        await workerRow.Locator("button:has-text('✏️')").ClickAsync();
        var editWorkerModal = _page.Locator("#editWorkerModal");
        await Assertions.Expect(editWorkerModal).ToBeVisibleAsync();
        await _page.FillAsync("#editWorkerMonthlyCost", "2350.00");
        await _page.ClickAsync("#editWorkerModal button[type='submit']");
        await Assertions.Expect(editWorkerModal).ToBeHiddenAsync();
        await Assertions.Expect(workerRow).ToContainTextAsync("2350.00 €");

        // Delete worker
        await workerRow.Locator("button:has-text('🗑️')").ClickAsync();
        await Assertions.Expect(workerRow).ToBeHiddenAsync();

        // 2. Cosméticos (Products) CRUD: Create, Edit, Delete
        await _page.ClickAsync("#tabBtnProducts");
        var productsTab = _page.Locator("#tabContentProducts");
        await Assertions.Expect(productsTab).ToBeVisibleAsync();

        await _page.ClickAsync("#tabContentProducts button:has-text('➕ Añadir')");
        var addProductModal = _page.Locator("#addProductModal");
        await Assertions.Expect(addProductModal).ToBeVisibleAsync();

        string uniqueProdName = $"Serum QA {DateTime.UtcNow.Ticks % 10000}";
        await _page.FillAsync("#productName", uniqueProdName);
        await _page.FillAsync("#productPrice", "35.50");
        await _page.FillAsync("#productCapacity", "100");
        await _page.SelectOptionAsync("#productUnit", "0");
        await _page.FillAsync("#productWaste", "5");
        await _page.ClickAsync("#addProductModal button[type='submit']");
        await Assertions.Expect(addProductModal).ToBeHiddenAsync();

        var prodRow = _page.Locator("#productsTableBody tr", new() { HasText = uniqueProdName });
        await Assertions.Expect(prodRow).ToBeVisibleAsync();

        // Edit product
        await prodRow.Locator("button:has-text('✏️')").ClickAsync();
        var editProductModal = _page.Locator("#editProductModal");
        await Assertions.Expect(editProductModal).ToBeVisibleAsync();
        await _page.FillAsync("#editProductFullPrice", "45.00");
        await _page.ClickAsync("#editProductModal button[type='submit']");
        await Assertions.Expect(editProductModal).ToBeHiddenAsync();
        await Assertions.Expect(prodRow).ToContainTextAsync("45.00 €");

        // Delete product
        await prodRow.Locator("button:has-text('🗑️')").ClickAsync();
        await Assertions.Expect(prodRow).ToBeHiddenAsync();
    }

    [Fact]
    [Trait("Category", "BrowserFeature")]
    [Trait("Case", "2_DeleteService")]
    public async Task Playwright_IdentifiedCase2_ServicesList_DeleteService()
    {
        Assert.NotNull(_page);
        await LoginAsGestorAsync();
        _page.Dialog += async (_, d) => await d.AcceptAsync();

        // Switch to Madrid where quota is available
        var centerSelect = _page.Locator("#centerSelect");
        await centerSelect.SelectOptionAsync("ctr-madrid-1");

        // Create a service to delete
        var btnCreate = _page.Locator("button:has-text('➕ Nuevo Escandallo')");
        await btnCreate.ClickAsync();

        var createModal = _page.Locator("#createModal");
        await Assertions.Expect(createModal).ToBeVisibleAsync();

        string serviceToDelete = $"Drenaje QA {DateTime.UtcNow.Ticks % 10000}";
        await _page.FillAsync("#createServiceName", serviceToDelete);
        await _page.FillAsync("#createDuration", "35");
        await _page.FillAsync("#createPvr", "48.00");

        await _page.ClickAsync("#createModal button[type='submit']");
        await Assertions.Expect(createModal).ToBeHiddenAsync();

        // Locate row and verify visible
        var targetRow = _page.Locator("#servicesTableBody tr", new() { HasText = serviceToDelete });
        await Assertions.Expect(targetRow).ToBeVisibleAsync();

        // Delete service
        var btnDelete = targetRow.Locator("button:has-text('🗑️ Borrar')");
        await btnDelete.ClickAsync();

        // Verify service row is removed
        await Assertions.Expect(targetRow).ToBeHiddenAsync();
    }

    [Fact]
    [Trait("Category", "BrowserFeature")]
    [Trait("Case", "3_AuditTrail")]
    public async Task Playwright_IdentifiedCase3_AuditTrail_CryptographicHistoryModal()
    {
        Assert.NotNull(_page);
        await LoginAsGestorAsync();
        _page.Dialog += async (_, d) => await d.AcceptAsync();

        // 1. Trigger an expense change (Update salon overhead parameters)
        var rentInput = _page.Locator("#inputRent");
        if (await rentInput.IsVisibleAsync())
        {
            await rentInput.FillAsync("350.00");
            await _page.ClickAsync("button:has-text('💾 Guardar Parámetros')");
        }

        // 2. Open audit modal
        var btnAudit = _page.Locator("button:has-text('📜 Histórico')");
        await btnAudit.ClickAsync();

        var auditModal = _page.Locator("#auditModal");
        await Assertions.Expect(auditModal).ToBeVisibleAsync();

        // 3. Verify center filter is present and select 'all'
        var centerFilter = _page.Locator("#auditCenterSelect");
        await Assertions.Expect(centerFilter).ToBeVisibleAsync();
        await centerFilter.SelectOptionAsync("all");

        // 4. Wait for entries in audit table
        var firstRow = _page.Locator("#auditTableBody tr").First;
        await firstRow.WaitForAsync();
        await Assertions.Expect(firstRow).ToBeVisibleAsync();

        // 5. Check SHA-256 chained hash code element
        var hashCell = firstRow.Locator("code");
        var hashText = await hashCell.InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(hashText));

        // 6. Close modal
        await _page.ClickAsync("#auditModal button:has-text('Cerrar')");
        await Assertions.Expect(auditModal).ToBeHiddenAsync();
    }

    [Fact]
    [Trait("Category", "BrowserFeature")]
    [Trait("Case", "4_CreateCenter")]
    public async Task Playwright_IdentifiedCase4_MultiTenant_CreateNewAestheticCenter()
    {
        Assert.NotNull(_page);
        await LoginAsGestorAsync();

        // Click "+ Nuevo Centro" in header
        var btnNewCenter = _page.Locator("#btnOpenNewCenter");
        await Assertions.Expect(btnNewCenter).ToBeVisibleAsync();
        await btnNewCenter.ClickAsync();

        var createCenterModal = _page.Locator("#createCenterModal");
        await Assertions.Expect(createCenterModal).ToBeVisibleAsync();

        string uniqueCenterName = $"Centro QA {DateTime.UtcNow.Ticks % 10000}";
        await _page.FillAsync("#newCenterNameInput", uniqueCenterName);
        await _page.ClickAsync("#createCenterModal button[type='submit']");

        // Verify modal hides and center is active
        await Assertions.Expect(createCenterModal).ToBeHiddenAsync();

        var badge = _page.Locator("#activeCenterBadgeText");
        await Assertions.Expect(badge).ToContainTextAsync(uniqueCenterName);

        var centerSelect = _page.Locator("#centerSelect");
        await Assertions.Expect(centerSelect).ToContainTextAsync(uniqueCenterName);
    }

    [Fact]
    [Trait("Category", "BrowserFeature")]
    [Trait("Case", "5_PurgeDemo")]
    public async Task Playwright_IdentifiedCase5_DemoMode_PurgeDemoDataCleansStateAndQuota()
    {
        Assert.NotNull(_page);
        await LoginAsGestorAsync();
        _page.Dialog += async (_, d) => await d.AcceptAsync();

        // First load demo data to ensure services exist
        var btnLoadDemo = _page.Locator("button:has-text('🚀 Cargar datos Demo')");
        await btnLoadDemo.ClickAsync();
        await _page.Locator("#servicesTableBody tr").First.WaitForAsync();

        // Now click Purge Demo
        var btnPurge = _page.Locator("button:has-text('🗑️ Limpiar Demo')");
        await btnPurge.ClickAsync();

        // Verify state becomes empty and quota shows 0/2
        var emptyState = _page.Locator("#stateEmpty");
        await Assertions.Expect(emptyState).ToBeVisibleAsync();

        var quotaBadge = _page.Locator("#quotaBadge");
        await Assertions.Expect(quotaBadge).ToContainTextAsync("0/2");

        // Restore demo data so subsequent tests have nominal services
        await btnLoadDemo.ClickAsync();
        await _page.Locator("#servicesTableBody tr").First.WaitForAsync();
    }

    [Fact]
    [Trait("Category", "BrowserFeature")]
    [Trait("Case", "6_SubscribeCancelPro")]
    public async Task Playwright_IdentifiedCase6_Subscription_SubscribeAndCancelProPlan()
    {
        Assert.NotNull(_page);
        await LoginAsGestorAsync();
        _page.Dialog += async (_, d) => await d.AcceptAsync();

        // Switch to Madrid center
        var centerSelect = _page.Locator("#centerSelect");
        await centerSelect.SelectOptionAsync("ctr-madrid-1");

        var quotaBadge = _page.Locator("#quotaBadge");
        var btnSub = _page.Locator("#btnSubscribePro");
        var btnCancel = _page.Locator("#btnCancelPro");

        // If already Pro from a prior run, cancel it first to baseline Free
        if (await btnCancel.IsVisibleAsync())
        {
            await btnCancel.ClickAsync();
        }

        // Subscribe to Pro
        await Assertions.Expect(btnSub).ToBeVisibleAsync();
        await btnSub.ClickAsync();

        // Verify upgraded to Pro
        await Assertions.Expect(quotaBadge).ToContainTextAsync("Plan Pro Ilimitado");
        await Assertions.Expect(btnSub).ToBeHiddenAsync();
        await Assertions.Expect(btnCancel).ToBeVisibleAsync();

        // Cancel Pro
        await btnCancel.ClickAsync();

        // Verify reverted to Free
        await Assertions.Expect(quotaBadge).ToContainTextAsync("Plan Free");
        await Assertions.Expect(btnSub).ToBeVisibleAsync();
        await Assertions.Expect(btnCancel).ToBeHiddenAsync();
    }

    [Fact]
    [Trait("Category", "BrowserFeature")]
    [Trait("Case", "7_UserRegistration")]
    public async Task Playwright_IdentifiedCase7_UserManagement_RegisterNewAccountAndCenter()
    {
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

        var loginModal = _page.Locator("#loginModal");
        await Assertions.Expect(loginModal).ToBeVisibleAsync();

        // Switch to register tab
        await _page.ClickAsync("#tabBtnRegister");
        var regForm = _page.Locator("#registerForm");
        await Assertions.Expect(regForm).ToBeVisibleAsync();

        string uniqueEmail = $"qa_user_{DateTime.UtcNow.Ticks % 100000}@estetica.com";
        string uniqueCenter = $"Salón VIP QA {DateTime.UtcNow.Ticks % 10000}";

        await _page.FillAsync("#registerEmail", uniqueEmail);
        await _page.FillAsync("#registerPassword", "SecurePass123!");
        await _page.FillAsync("#registerCenterName", uniqueCenter);
        await _page.ClickAsync("#btnRegisterSubmit");

        // Verify login modal closes, logout button appears and active center badge matches
        await Assertions.Expect(loginModal).ToBeHiddenAsync();
        var btnLogout = _page.Locator("#btnLogout");
        await Assertions.Expect(btnLogout).ToBeVisibleAsync();

        var badge = _page.Locator("#activeCenterBadgeText");
        await Assertions.Expect(badge).ToContainTextAsync(uniqueCenter);
    }

    [Fact]
    [Trait("Category", "BrowserFeature")]
    [Trait("Case", "8_LogsTraces")]
    public async Task Playwright_IdentifiedCase8_Observability_RealtimeLogsAndTracesConsole()
    {
        Assert.NotNull(_page);
        await LoginAsGestorAsync();

        // Open logs modal
        var btnLogs = _page.Locator("button:has-text('🔍 Logs y Trazas')");
        await btnLogs.ClickAsync();

        var logsModal = _page.Locator("#logsModal");
        await Assertions.Expect(logsModal).ToBeVisibleAsync();

        // Verify logs console loads structured log entries
        var logsConsole = _page.Locator("#logsConsole");
        await Assertions.Expect(logsConsole).ToContainTextAsync("[INFORMATION]");

        // Click refresh logs button
        var btnRefresh = _page.Locator("#logsModal button:has-text('🔄 Refrescar Logs')");
        await btnRefresh.ClickAsync();
        await Assertions.Expect(logsConsole).ToContainTextAsync("[INFORMATION]");

        // Close modal
        await _page.ClickAsync("#logsModal button:has-text('Cerrar')");
        await Assertions.Expect(logsModal).ToBeHiddenAsync();
    }
}

