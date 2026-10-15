using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Escandallo.Core.Contracts;

namespace Escandallo.Core.Services;

/// <summary>
/// Repositorio inmutable de pistas de auditoría con encadenamiento criptográfico SHA-256 (ADR-004, SEC-REQ-AUDIT-IMMUTABLE-001).
/// </summary>
public class AuditTrailRepository : IAuditTrailRepository
{
    private readonly ConcurrentDictionary<string, List<AuditEntry>> _historyByCenter = new();
    private readonly object _lock = new();

    public void RecordEntry(AuditEntry entry)
    {
        lock (_lock)
        {
            var list = _historyByCenter.GetOrAdd(entry.CenterId, _ => new List<AuditEntry>());

            if (string.IsNullOrEmpty(entry.ChainedHashSha256))
            {
                string previousHash = list.LastOrDefault()?.ChainedHashSha256 ?? new string('0', 64);
                string dataToHash = $"{previousHash}:{entry.CenterId}:{entry.ActorId}:{entry.TimestampUtc:O}:{entry.PreviousValue}:{entry.NewValue}:{entry.ChangeType}";
                entry.ChainedHashSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dataToHash))).ToLowerInvariant();
            }

            list.Add(entry);
        }
    }

    public IEnumerable<AuditEntry> GetHistory(string centerId, string? resourceId = null)
    {
        if (_historyByCenter.TryGetValue(centerId, out var list))
        {
            lock (_lock)
            {
                return list.ToList();
            }
        }

        return Enumerable.Empty<AuditEntry>();
    }
}
