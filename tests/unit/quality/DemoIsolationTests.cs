using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Quality;

public class DemoIsolationTests
{
    private readonly IDemoManager _demoManager = new DemoManager();

    [Fact]
    public void Nominal_NewCenterCreation_StartsStrictlyEmptyWithZeroDemoData()
    {
        // Arrange
        string newCenterId = "ctr-innovacion-new";

        // Act
        var (hasDemo, demoCount, realCount) = _demoManager.GetStatus(newCenterId);

        // Assert: SEC-REQ-DEMO-ISOLATION-001 and FR-DEMO-MODE-001
        Assert.False(hasDemo);
        Assert.Equal(0, demoCount);
        Assert.Equal(0, realCount);
    }

    [Fact]
    public void Security_DemoPurge_RemovesOnlyDemoRecordsAndPreservesRealRecords()
    {
        // Arrange: Center has 3 real cosmetics and 5 demo cosmetics
        string centerId = "ctr-mixed-data";

        // Act: Request transactional purge
        int deletedCount = _demoManager.PurgeDemoData(centerId);

        // Assert: Exactly the 5 demo items are purged, real items unaffected
        Assert.Equal(5, deletedCount);

        var (hasDemo, demoCount, realCount) = _demoManager.GetStatus(centerId);
        Assert.False(hasDemo);
        Assert.Equal(0, demoCount);
        Assert.Equal(3, realCount);
    }
}
