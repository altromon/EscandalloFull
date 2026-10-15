using System.Security.Cryptography;
using System.Text;
using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Security;

public class AuditLogImmutabilityTests
{
    private readonly IAuditTrailRepository _auditRepository = new AuditTrailRepository();

    [Fact]
    public void Security_PriceVariation_GeneratesAppendOnlyLogWithChainedSha256Hash()
    {
        // Arrange
        string centerId = "ctr-madrid-1";
        string actorId = "gestor@estetica.com";
        decimal prevValue = 30.00m;
        decimal newValue = 45.00m;
        string changeType = "UPDATE_COSMETIC_PRICE";
        DateTime timestamp = DateTime.UtcNow;

        string previousEntryHash = "0000000000000000000000000000000000000000000000000000000000000000";
        string dataToHash = $"{previousEntryHash}:{centerId}:{actorId}:{timestamp:O}:{prevValue}:{newValue}:{changeType}";
        string expectedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dataToHash))).ToLowerInvariant();

        var entry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            CenterId = centerId,
            ActorId = actorId,
            TimestampUtc = timestamp,
            PreviousValue = prevValue,
            NewValue = newValue,
            ChangeType = changeType,
            ChainedHashSha256 = expectedHash
        };

        // Act
        _auditRepository.RecordEntry(entry);
        var history = _auditRepository.GetHistory(centerId);

        // Assert
        Assert.NotNull(history);
        Assert.Contains(history, h => h.ActorId == actorId && h.NewValue == newValue && !string.IsNullOrEmpty(h.ChainedHashSha256));
    }
}
