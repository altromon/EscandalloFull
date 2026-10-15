using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Bdd.StepDefinitions;

public class ServiceMarginSteps
{
    private readonly IEscandalloCostEngine _costEngine = new EscandalloCostEngine();
    private readonly IMarginCalculator _marginCalculator = new MarginCalculator();

    [Fact]
    [Trait("Category", "FR-SERVICE-MARGIN-001")]
    [Trait("Type", "AutomatedRegression")]
    public void Scenario_ExactEscandalloAndPriceSuggestionWithGeneralVat()
    {
        // Given un servicio "Mesodermolifting" con duración 60 minutos e IVA al 21%
        int duration = 60;
        decimal taxRate = 21.0m;

        // And gastos generales por minuto de 0.08286 EUR
        decimal overheadPerMin = 0.08285985m;

        // And profesional asignado "Encargado" con coste por minuto de 0.27273 EUR
        decimal workerPerMin = 0.27272727m;

        // And un coste total de productos cosméticos utilizados de 13.37723 EUR
        decimal cosmeticsCost = 13.37723m;

        // When el sistema calcula el escandallo total del servicio
        decimal totalEscandallo = _costEngine.CalculateEscandallo(duration, overheadPerMin, workerPerMin, cosmeticsCost);

        // Then el escandallo total resultante es 34.71 EUR
        Assert.Equal(34.71m, Math.Round(totalEscandallo, 2));

        // And el precio base sugerido con 50% de beneficio es 52.07 EUR
        var (suggestedBase, suggestedPvp) = _marginCalculator.CalculateSuggestedPrices(totalEscandallo, 50.0m, taxRate);
        Assert.Equal(52.07m, Math.Round(suggestedBase, 2));

        // And el PVP sugerido con 21% de IVA es 63.00 EUR
        Assert.Equal(63.00m, Math.Round(suggestedPvp, 2));
    }

    [Fact]
    [Trait("Category", "FR-SERVICE-MARGIN-001")]
    [Trait("Type", "AutomatedRegression")]
    public void Scenario_MedicalAestheticTreatment_VatExempt()
    {
        // Given un servicio "Consulta Médica Dermatológica" con escandallo total 30.00 EUR e IVA al 0% ("exento")
        decimal totalEscandallo = 30.00m;
        decimal appliedPvr = 80.00m;
        decimal taxRate = 0.0m;

        // When el usuario establece un PVR de 80.00 EUR
        var (netMarginAbs, netMarginPct) = _marginCalculator.CalculateRealMargin(appliedPvr, totalEscandallo, taxRate);
        var color = _marginCalculator.DetermineTrafficLight(netMarginPct);

        // Then el margen neto absoluto es 50.00 EUR
        Assert.Equal(50.00m, netMarginAbs);

        // And el margen real sobre venta es 62.5%
        Assert.Equal(62.5m, netMarginPct);

        // And el semáforo visual asignado es "verde"
        Assert.Equal(TrafficLightColor.Verde, color);
    }

    [Theory]
    [Trait("Category", "FR-SERVICE-MARGIN-001")]
    [InlineData(5, 400.00, 66.12, 47.5, TrafficLightColor.Verde)]
    [InlineData(5, 300.00, 49.59, 30.0, TrafficLightColor.Amarillo)]
    [InlineData(5, 200.00, 33.06, -5.0, TrafficLightColor.Rojo)]
    public void ScenarioOutline_SessionPacksRentability(int sessions, decimal packPrice, decimal expectedSessionRevenue, decimal expectedMargin, TrafficLightColor expectedColor)
    {
        // Given un servicio con escandallo unitario de 34.71 EUR e IVA al 21%
        decimal totalEscandallo = 34.71m;
        decimal taxRate = 21.0m;

        // When se define un bono de <num_sesiones> sesiones con precio total de <precio_bono_con_iva> EUR
        var (sessionRevenue, marginPct) = _marginCalculator.CalculatePackMargin(packPrice, sessions, totalEscandallo, taxRate);
        var color = _marginCalculator.DetermineTrafficLight(marginPct);

        // Then el ingreso efectivo por sesión sin IVA es <ingreso_sesion_sin_iva> EUR
        Assert.Equal(expectedSessionRevenue, Math.Round(sessionRevenue, 2));

        // And el margen porcentual resultante del bono es <margen_bono_pct>%
        Assert.Equal(expectedMargin, Math.Round(marginPct, 1));

        // And el color de semáforo para el bono es <color_semaforo>
        Assert.Equal(expectedColor, color);
    }

    [Fact]
    [Trait("Category", "FR-SERVICE-MARGIN-001")]
    public void Scenario_DetectionAndExplicitAlertOfSellingAtLoss()
    {
        // Given un servicio con escandallo total de 34.71 EUR e IVA al 21%
        decimal totalEscandallo = 34.71m;
        decimal appliedPvr = 30.00m;
        decimal taxRate = 21.0m;

        // When el usuario establece un PVR con IVA de 30.00 EUR
        var (netMarginAbs, netMarginPct) = _marginCalculator.CalculateRealMargin(appliedPvr, totalEscandallo, taxRate);
        var color = _marginCalculator.DetermineTrafficLight(netMarginPct);
        string? warning = _marginCalculator.FormatLossWarning(netMarginAbs);

        // Then el ingreso neto sin IVA es 24.79 EUR
        decimal revenueWithoutVat = Math.Round(appliedPvr / (1m + taxRate / 100m), 2);
        Assert.Equal(24.79m, revenueWithoutVat);

        // And el beneficio neto absoluto es -9.92 EUR
        Assert.Equal(-9.92m, Math.Round(netMarginAbs, 2));

        // And el margen real sobre venta es -40.0%
        Assert.Equal(-40.0m, Math.Round(netMarginPct, 1));

        // And el semáforo visual asignado es "rojo"
        Assert.Equal(TrafficLightColor.Rojo, color);

        // And el sistema emite el mensaje de advertencia
        Assert.Equal("Atención: Estás perdiendo 9.92 € por cada sesión", warning);
    }

    [Fact]
    [Trait("Category", "FR-SERVICE-MARGIN-001")]
    public void Scenario_SuccessfulServiceDuplicationAndParameterCloning()
    {
        var repo = new EscandalloRepository();
        var subMgr = new SubscriptionManager();
        string centerId = "ctr-radiante-1";

        var original = repo.GetByCenter(centerId).First(x => x.ServiceName == "Mesodermolifting");
        Assert.Equal(3, original.Consumables.Count);
        Assert.Single(original.Packs);

        string newId = Guid.NewGuid().ToString();
        subMgr.ValidateAndRegisterEscandallo(centerId, newId);

        var cloned = new CompleteEscandallo
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
                TrafficLightColor = p.TrafficLightColor
            }).ToList()
        };
        repo.Save(centerId, cloned);

        Assert.Equal("Copia de Mesodermolifting", cloned.ServiceName);
        Assert.Equal(original.DurationMinutes, cloned.DurationMinutes);
        Assert.Equal(original.WorkerCategory, cloned.WorkerCategory);
        Assert.Equal(original.Consumables.Count, cloned.Consumables.Count);
        Assert.Equal(original.Packs.Count, cloned.Packs.Count);
    }

    [Fact]
    [Trait("Category", "FR-SERVICE-MARGIN-001")]
    public void Scenario_DeactivateEscandalloToReleaseFreemiumQuota()
    {
        var repo = new EscandalloRepository();
        var subMgr = new SubscriptionManager();
        string centerId = "ctr-luz-belleza";

        var statusBefore = subMgr.GetStatus(centerId);
        Assert.Equal(2, statusBefore.ActiveEscandallos);
        Assert.False(subMgr.CanCreateEscandallo(centerId));

        var service = repo.GetByCenter(centerId).First(x => x.IsActive);
        service.IsActive = false;
        repo.Save(centerId, service);
        subMgr.DecrementActiveEscandallos(centerId);

        var statusAfter = subMgr.GetStatus(centerId);
        Assert.Equal(1, statusAfter.ActiveEscandallos);
        Assert.True(subMgr.CanCreateEscandallo(centerId));
    }
}
