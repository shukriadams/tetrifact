using Tetrifact.Core.Porter_Packages.Madscience.Loggger;
using System;
using System.Collections.Generic;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Implements a logger that catches and exposes all entries in LogEntries collection.
    /// </summary>
    public class TestLogger : ILoggger
    {
        public List<string> LogEntries = new List<string>();

        /// <summary>
        /// Helper method - returns true if any of the log entries contains the string fragment
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool ContainsFragment(string fragment)
        { 
            foreach (string logEntry in this.LogEntries)
                if (!string.IsNullOrEmpty(logEntry) && logEntry.Contains(fragment))
                    return true;

            return false;
        }

        public int VerbosityThreshold { get; set; }

        public void Dispose()
        {

        }

        public void Error(object source, string message)
        {
            LogEntries.Add($"Error:{source}:{message}");
        }

        public void Error(object source, object exception)
        {
            LogEntries.Add($"Error:{source}:{exception}");
        }

        public void Error(object source, string message, object exception)
        {
            LogEntries.Add($"Error:{source}:{exception}");
        }

        public void Warn(object source, string message)
        {
            LogEntries.Add($"Warn:{source}:{message}");
        }

        public void Warn(object source, object exception)
        {
            LogEntries.Add($"Warn:{source}:{exception}");
        }

        public void Warn(object source, string message, object exception)
        {
            LogEntries.Add($"Warn:{source}:{message}:{exception}");
        }

        public void Status(object source, string message, int verbosity = 0)
        {
            LogEntries.Add($"Status:{source}:{message}:{verbosity}");
        }

        public void Status(string source, string message, int verbosity = 0)
        {
            LogEntries.Add($"Status:{source}:{message}:{verbosity}");
        }

        public void Debug(object source, string message, int verbosity = 0)
        {
            LogEntries.Add($"Debug:{source}:{message}:{verbosity}");
        }

        public void Debug(string source, string message, int verbosity = 0)
        {
            LogEntries.Add($"Debug:{source}:{message}:{verbosity}");
        }
        
        public void Trace(object source, string message, int verbosity = 0)
        {
            LogEntries.Add($"Trace:{source}:{message}:{verbosity}");
        }

        public void Trace(string source, string message, int verbosity)
        {
            LogEntries.Add($"Trace:{source}:{message}:{verbosity}");
        }

    }
}
