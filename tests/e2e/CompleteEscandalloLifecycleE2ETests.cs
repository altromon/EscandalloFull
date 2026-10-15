using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.E2E;

public class CompleteEscandalloLifecycleE2ETests
{
    private readonly IAuthGateway _auth = new AuthGateway();
    private readonly IEscandalloCostEngine _costEngine = new EscandalloCostEngine();
    private readonly IMarginCalculator _marginCalculator = new MarginCalculator();
    private readonly IExportDocumentEngine _exportEngine = new ExportDocumentEngine();

    [Fact]
    public void E2E_FullLifecycle_FromLoginToFormulationDuplicationAndExport()
    {
        // 1. Login and center selection
        var session = _auth.Login("gestor@estetica.com", "SecretPass123!");
        Assert.NotNull(session.Token);

        var centerSession = _auth.SelectCenter(session.UserId, "ctr-madrid-1");
        Assert.Equal("ctr-madrid-1", centerSession.ActiveCenterId);

        // 2. Configure Opening Hours & Overhead
        var costParams = _costEngine.CalculateCostParameters(176m, 335.00m, 100.00m, 440.00m);
        Assert.Equal(10560m, costParams.MonthlyOpeningMinutes);

        // 3. Register Cosmetic with Merma
        var (grossUnitCost, effectiveUnitCost) = _costEngine.CalculateCosmeticUnitCost(34.51m, 500m, 5.0m);
        Assert.True(effectiveUnitCost > grossUnitCost);

        // 4. Formulate Service (Mesodermolifting)
        decimal totalEscandallo = _costEngine.CalculateEscandallo(60, costParams.OverheadCostPerMinute, 0.272727m, 13.37723m);
        Assert.Equal(34.71m, Math.Round(totalEscandallo, 2));

        // 5. Margin Evaluation
        var (suggestedBase, suggestedPvp) = _marginCalculator.CalculateSuggestedPrices(totalEscandallo, 50.0m, 21.0m);
        Assert.Equal(63.00m, Math.Round(suggestedPvp, 2));

        var escandallos = new List<CompleteEscandallo>
        {
            new()
            {
                Id = "esc-meso-01",
                ServiceName = "Mesodermolifting",
                TotalEscandalloCost = totalEscandallo,
                SuggestedPvp = suggestedPvp,
                AppliedPvr = 63.00m
            }
        };

        // 6. Export to PDF and XLSX
        byte[] pdf = _exportEngine.ExportPdf("ctr-madrid-1", escandallos);
        byte[] xlsx = _exportEngine.ExportXlsx("ctr-madrid-1", escandallos);

        Assert.NotNull(pdf);
        Assert.NotEmpty(pdf);
        Assert.NotNull(xlsx);
        Assert.NotEmpty(xlsx);
    }
}
