using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.E2E;

public class DemoModeE2ETests
{
    private readonly IDemoManager _demoManager = new DemoManager();

    [Fact]
    public void E2E_DemoDataLifecycle_StartsClean_LoadsStandardSeed_AndPurgesCleanly()
    {
        string centerId = "ctr-clean-e2e";

        // 1. Initial Empty State
        var initial = _demoManager.GetStatus(centerId);
        Assert.False(initial.hasDemoData);
        Assert.Equal(0, initial.demoCount);
        Assert.Equal(0, initial.realCount);

        // 2. Load standard demo seed
        _demoManager.LoadDemoData(centerId, overrideIfRealExists: false);

        var loaded = _demoManager.GetStatus(centerId);
        Assert.True(loaded.hasDemoData);
        Assert.Equal(2, loaded.demoCount); // Cleasing Milk & Mesodermolifting

        // 3. Purge demo data
        int purged = _demoManager.PurgeDemoData(centerId);
        Assert.Equal(2, purged);

        var finalState = _demoManager.GetStatus(centerId);
        Assert.False(finalState.hasDemoData);
        Assert.Equal(0, finalState.demoCount);
    }
}
