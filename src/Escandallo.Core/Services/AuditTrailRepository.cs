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
        lock (_lock)
        {
            if (string.IsNullOrEmpty(centerId) || centerId.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (_historyByCenter.IsEmpty)
                {
                    EnsureGenesis("ctr-madrid-1");
                }
                var all = _historyByCenter.Values.SelectMany(x => x).OrderBy(x => x.TimestampUtc).ToList();
                if (!string.IsNullOrEmpty(resourceId))
                {
                    all = all.Where(x => x.ChangeType.Contains(resourceId, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                return all;
            }

            if (!_historyByCenter.ContainsKey(centerId) || _historyByCenter[centerId].Count == 0)
            {
                EnsureGenesis(centerId);
            }

            if (_historyByCenter.TryGetValue(centerId, out var list))
            {
                var result = list.ToList();
                if (!string.IsNullOrEmpty(resourceId))
                {
                    result = result.Where(x => x.ChangeType.Contains(resourceId, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                return result;
            }

            return Enumerable.Empty<AuditEntry>();
        }
    }

    private void EnsureGenesis(string centerId)
    {
        var list = _historyByCenter.GetOrAdd(centerId, _ => new List<AuditEntry>());
        if (list.Count == 0)
        {
            var genesis = new AuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                CenterId = centerId,
                ActorId = "System",
                TimestampUtc = DateTime.UtcNow,
                PreviousValue = 0,
                NewValue = 0,
                ChangeType = "GENESIS:Auditoría inicializada (Línea base criptográfica)"
            };
            string previousHash = new string('0', 64);
            string dataToHash = $"{previousHash}:{genesis.CenterId}:{genesis.ActorId}:{genesis.TimestampUtc:O}:{genesis.PreviousValue}:{genesis.NewValue}:{genesis.ChangeType}";
            genesis.ChainedHashSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dataToHash))).ToLowerInvariant();
            list.Add(genesis);
        }
    }
}

