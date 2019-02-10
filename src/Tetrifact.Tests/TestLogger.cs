using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Implements a logger that catches and exposes all entries in LogEntries collection.
    /// </summary>
    public class TestLogger<T> : ILogger<T>
    {
        public List<string> LogEntries = new List<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LogEntries.Add($"{logLevel.ToString()}:{formatter(state, exception)}");
        }
    }
}
