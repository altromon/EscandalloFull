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

    [Fact]
    [Trait("Category", "BrowserSmoke")]
    public async Task SmokeTest_NavigateToHome_DisplaysNavbarAndLoadsServices()
    {
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

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
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

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
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

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
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

        // Wait for initial load
        await _page.Locator("#servicesTableBody tr").First.WaitForAsync();

        // Ensure we are in Madrid (1/2 quota)
        var centerSelect = _page.Locator("#centerSelect");
        await centerSelect.SelectOptionAsync("ctr-madrid-1");

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
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

        // Wait for initial load
        await _page.Locator("#servicesTableBody tr").First.WaitForAsync();

        // Switch to ctr-radiante-1 (starts with 1 service and 1/2 quota)
        var centerSelect = _page.Locator("#centerSelect");
        await centerSelect.SelectOptionAsync("ctr-radiante-1");

        // Wait for Radiante to load
        var badge = _page.Locator("#activeCenterBadgeText");
        await Assertions.Expect(badge).ToContainTextAsync("Centro Radiante");

        var initialRows = await _page.Locator("#servicesTableBody tr").CountAsync();
        Assert.Equal(1, initialRows);

        // Click duplicate button on first row
        var btnDuplicate = _page.Locator("#servicesTableBody tr button:has-text('📋 Duplicar')").First;
        await btnDuplicate.ClickAsync();

        // Table should now have 2 rows and contain "Copia de Mesodermolifting"
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
        Assert.NotNull(_page);
        await _page.GotoAsync(_fixture.WebBaseUrl);

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
}

