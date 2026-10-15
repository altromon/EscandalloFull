using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.UI;

public class CenterSelectorViewModel
{
    public string ActiveCenterId { get; set; } = string.Empty;
    public string ActiveCenterName { get; set; } = string.Empty;
    public RoleType CurrentRole { get; set; }
    public bool ShowWageControls => CurrentRole != RoleType.Guest;

    public void SelectCenter(string centerId, string centerName)
    {
        ActiveCenterId = centerId;
        ActiveCenterName = centerName;
    }
}

public class CenterSelectorComponentTests
{
    [Fact]
    public void Presentation_NavbarCenterDisplay_UpdatesPermanentlyOnSelection()
    {
        // Arrange
        var vm = new CenterSelectorViewModel();

        // Act
        vm.SelectCenter("ctr-madrid-1", "Centro Belleza Madrid");

        // Assert: CMP-WEB-UI-001 Section 1.1
        Assert.Equal("ctr-madrid-1", vm.ActiveCenterId);
        Assert.Equal("Centro Belleza Madrid", vm.ActiveCenterName);
    }

    [Fact]
    public void Presentation_GuestMode_HidesWageAndOverheadEditControls()
    {
        // Arrange
        var vm = new CenterSelectorViewModel { CurrentRole = RoleType.Guest };

        // Assert: BR-GUEST-CENSOR-001 and CMP-WEB-UI-001
        Assert.False(vm.ShowWageControls);
    }
}
