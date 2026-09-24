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

        void Error(object source, string message)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Error}:{source}:{message}"
            );
        }

        void Error(object source, object exception)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Error}:{source}:{exception}"
            );
        }

        void Error(object source, string message, object exception)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Error}:{source}:{exception}"
            );
        }

        void Warn(object source, string message)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Warn}:{source}:{message}"
            );
        }

        void Warn(object source, object exception)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Warn}:{source}:{exception}"
            );
        }

        void Warn(object source, string message, object exception)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Warn}:{source}:{message}:{exception}"
            );
        }

        void Status(object source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Status}:{source}:{message}:{verbosity}"
            );
        }

        void Status(string source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Status}:{source}:{message}:{verbosity}"
            );
        }

        void Debug(object source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Debug}:{source}:{message}:{verbosity}"
            );
        }

        void Debug(string source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Debug}:{source}:{message}:{verbosity}"
            );
        }
        
        void Trace(object source, string message, int verbosity = 0)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Trace}:{source}:{message}:{verbosity}"
            );
        }

        void Trace(string source, string message, int verbosity)
        {
            File.WriteAllText(
                Path.Join(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()), 
                $"{LogLevel.Trace}:{source}:{message}:{verbosity}"
            );
        }        
    }
}
