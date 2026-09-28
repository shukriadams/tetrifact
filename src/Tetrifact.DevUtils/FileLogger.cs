using Tetrifact.Core.Porter_Packages.Madscience.Loggger;
using System;
using System.Collections.Generic;
using System.IO;

namespace Tetrifact.DevUtils
{
    /// <summary>
    /// Implements a logger that writes each log entry to a unique file. This is for testing / dev only!
    /// </summary>
    public class FileLogger : ILoggger
    {
        public int VerbosityThreshold { get; set; }

        public void Dispose()
        {

        }

        public void Error(object source, string message)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"ERROR:{source}:{message}"
            );
        }

        public void Error(object source, object exception)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"ERROR::{source}:{exception}"
            );
        }

        public void Error(object source, string message, object exception)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"ERROR::{source}:{exception}"
            );
        }

        public void Warn(object source, string message)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"WARN:{source}:{message}"
            );
        }

        public void Warn(object source, object exception)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"WARN:{source}:{exception}"
            );
        }

        public void Warn(object source, string message, object exception)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"WARN:{source}:{message}:{exception}"
            );
        }

        public void Status(object source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"STATUS:{source}:{message}:{verbosity}"
            );
        }

        public void Status(string source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"STATUS:{source}:{message}:{verbosity}"
            );
        }

        public void Debug(object source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"DEBUG:{source}:{message}:{verbosity}"
            );
        }

        public void Debug(string source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"DEBUG:{source}:{message}:{verbosity}"
            );
        }
        
        public void Trace(object source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"TRACE:{source}:{message}:{verbosity}"
            );
        }

        public void Trace(string source, string message, int verbosity)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"TRACE:{source}:{message}:{verbosity}"
            );
        }        
    }
}
