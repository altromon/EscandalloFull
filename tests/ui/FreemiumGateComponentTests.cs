using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.UI;

public class FreemiumQuotaViewModel
{
    public int ActiveEscandallos { get; set; }
    public int MaxAllowed { get; set; } = 2;
    public PlanType Plan { get; set; } = PlanType.Free;
    public bool ShowUpgradeModal { get; set; }

    public string QuotaBadgeText => $"Escandallos activos: {ActiveEscandallos}/{MaxAllowed} (Plan {Plan})";

    public void RequestCreateNew()
    {
        if (Plan == PlanType.Free && ActiveEscandallos >= MaxAllowed)
        {
            ShowUpgradeModal = true;
        }
    }
}

public class FreemiumGateComponentTests
{
    [Fact]
    public void Presentation_QuotaBadgeText_DisplaysExplicitActiveCountAndPlan()
    {
        // Arrange
        var vm = new FreemiumQuotaViewModel { ActiveEscandallos = 1, MaxAllowed = 2, Plan = PlanType.Free };

        // Act & Assert
        Assert.Equal("Escandallos activos: 1/2 (Plan Free)", vm.QuotaBadgeText);
    }

    [Fact]
    public void Interaction_NewEscandalloAtFullCapacity_TriggersUpgradeModal()
    {
        // Arrange: Free center at 2/2 capacity
        var vm = new FreemiumQuotaViewModel { ActiveEscandallos = 2, MaxAllowed = 2, Plan = PlanType.Free };

        // Act
        vm.RequestCreateNew();

        // Assert: CMP-WEB-UI-001 Section 1.5
        Assert.True(vm.ShowUpgradeModal);
    }
}
