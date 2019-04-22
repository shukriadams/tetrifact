using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tetrifact.DevUtils
{
    /// <summary>
    /// Implements a logger that writes each log entry to a unique file. This is for testing / dev only!
    /// </summary>
    public class FileLogger<T> : ILogger<T>
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
            File.WriteAllText(Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), $"{logLevel.ToString()}:{formatter(state, exception)}");
        }
    }
}
