using Microsoft.Extensions.Logging;

namespace course_service.tests;

public class NoOpLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new NoOpLogger();
    public void Dispose() { }

    private class NoOpLogger : ILogger
    {
        IDisposable ILogger.BeginScope<TState>(TState state) => new NoOpDisposable();
        public bool IsEnabled(LogLevel logLevel) => false;
        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

        private class NoOpDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
