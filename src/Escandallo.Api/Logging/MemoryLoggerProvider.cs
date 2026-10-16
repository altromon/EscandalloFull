using Escandallo.Core.Services;
using Microsoft.Extensions.Logging;

namespace Escandallo.Api.Logging;

public class MemoryLoggerProvider : ILoggerProvider
{
    private readonly ISystemLogBuffer _buffer;

    public MemoryLoggerProvider(ISystemLogBuffer buffer)
    {
        _buffer = buffer;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new MemoryLogger(categoryName, _buffer);
    }

    public void Dispose() { }

    private class MemoryLogger : ILogger
    {
        private readonly string _category;
        private readonly ISystemLogBuffer _buffer;

        public MemoryLogger(string category, ISystemLogBuffer buffer)
        {
            _category = category;
            _buffer = buffer;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string message = formatter(state, exception);
            if (exception != null)
            {
                message += $" | Ex: {exception.Message}";
            }

            string? traceId = System.Diagnostics.Activity.Current?.Id ?? System.Diagnostics.Activity.Current?.TraceId.ToString();

            _buffer.Enqueue(new LogMessageEntry(
                DateTime.UtcNow,
                logLevel.ToString().ToUpperInvariant(),
                _category,
                message,
                traceId));
        }
    }
}
