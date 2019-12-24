using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Tetrifact.Dev
{
    /// <summary>
    /// Shim logger that catches and exposes all logged data.
    /// /// </summary>
    public class MemoryLogger<T> : ILogger<T>
    {
        #region FIELDS

        /// <summary>
        /// Use to retrieve current instance of shim.
        /// </summary>
        public static MemoryLogger<T> Instance;

        private List<string> _logEntries = new List<string>();

        public IEnumerable<string> Entries { get { return _logEntries.AsReadOnly(); } }

        #endregion

        #region CTORS

        public MemoryLogger()
        {
            Instance = this;
        }

        #endregion

        #region METHODS


        public void Clear() 
        {
            this._logEntries.Clear();
        }

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
            _logEntries.Add($"{logLevel.ToString()}:{formatter(state, exception)}");
        }

        #endregion
    }
}
