using System.Collections.Concurrent;

namespace Escandallo.Core.Services;

public record LogMessageEntry(
    DateTime TimestampUtc,
    string LogLevel,
    string Category,
    string Message,
    string? TraceId = null);

public interface ISystemLogBuffer
{
    void Enqueue(LogMessageEntry entry);
    IEnumerable<LogMessageEntry> GetRecentLogs(int limit = 100);
}

public class SystemLogBuffer : ISystemLogBuffer
{
    private readonly ConcurrentQueue<LogMessageEntry> _queue = new();
    private const int MaxEntries = 500;

    public void Enqueue(LogMessageEntry entry)
    {
        _queue.Enqueue(entry);
        while (_queue.Count > MaxEntries && _queue.TryDequeue(out _))
        {
        }
    }

    public IEnumerable<LogMessageEntry> GetRecentLogs(int limit = 100)
    {
        return _queue.Reverse().Take(Math.Min(limit, 200)).ToList();
    }
}
