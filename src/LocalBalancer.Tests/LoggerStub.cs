using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace LocalBalancer.Tests
{
    public class LoggerStub<T> : ILogger<T>, IDisposable
    {
        public List<LogEntry> Logs { get; } = new List<LogEntry>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            this.Logs.Add(new LogEntry(logLevel, eventId, exception, message));
        }

        public sealed class LogEntry
        {
            public LogEntry(LogLevel logLevel, EventId eventId, Exception exception, string message)
            {
                this.LogLevel = logLevel;
                this.EventId = eventId;
                this.Exception = exception;
                this.Message = message;
            }

            public EventId EventId { get; }

            public Exception Exception { get; }

            public LogLevel LogLevel { get; }

            public string Message { get; }
        }
    }
}