using System;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tetrifact.Web
{
    public class Logger<T> : ILogger<T>
    {
        private readonly ILogger _logWriter;
        private readonly ISettings _settings;
        
        public Logger(ILogger logWriter, ISettings settings)
        {
            _settings = settings;
            _logWriter = logWriter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _settings.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Critical)
                _logWriter.LogCritical($"{logLevel.ToString()}:{formatter(state, exception)}");
            else if (logLevel == LogLevel.Debug)
                _logWriter.LogDebug($"{logLevel.ToString()}:{formatter(state, exception)}");
            else if (logLevel == LogLevel.Error)
                _logWriter.LogError($"{logLevel.ToString()}:{formatter(state, exception)}");
            else if (logLevel == LogLevel.Information)
                _logWriter.LogInformation($"{logLevel.ToString()}:{formatter(state, exception)}");
            else if (logLevel == LogLevel.Trace)
                _logWriter.LogTrace($"{logLevel.ToString()}:{formatter(state, exception)}");
            else if (logLevel == LogLevel.Warning)
                _logWriter.LogWarning($"{logLevel.ToString()}:{formatter(state, exception)}");
        }

    }
}
