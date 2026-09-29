//PORTER-WRAPPER!
namespace Tetrifact.Core.Porter_Packages {
//PORTER-WRAPPER!


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace Madscience.Loggger
{
    /// <summary>
    /// A non-sucking log for the real world, designed to get around the stupid conventions that plague
    /// Dotnet's built-in logging system.
    /// 
    /// Features:
    /// 
    /// You can log out "status" messages, which are just normal events that doesn't sit on the 
    /// programmer-centric Trace-to-Error error scale. Status events are always written to log,
    /// and are there for auditing etc. 
    /// 
    /// You can set verbosity on any scale you want, and set a verbosity filter at at scale you want.
    /// 
    /// You can log out to console, debug console in Visual Studio (because VS doesn't support vanilla console out) 
    /// and file, with the same logger.
    /// 
    /// You don't need 500 separate logging sink nuget packages.
    /// 
    /// Your log isn't going to get automatically flooded with all ASP.Nets built in garbage if you drop to the 
    /// 
    /// Built in daily log rotation.
    /// 
    /// Thread-safe, singleton-ready, debounced writes to disk.
    /// </summary>
    public class Loggger : ILoggger
    {
        #region FIELDS

        private System.Timers.Timer _timer;

        private string _path;
        
        private IList<string> _buffer = new List<string>();

        private List<string> _writeBuffer = new List<string>();

        /// <summary>
        /// Set to true if a write operation is in progress. Used to prevent overlapping writes.
        /// </summary>
        private bool _writing;

        #endregion

        #region PROPERTIES
        
        public string DateFormat {get;set;} = "yy/MM/dd HH:mm:ss";

        /// <summary>
        /// Writes to system console. Default is true.
        /// </summary>
        public bool WriteToConsole { get; set; } = true;

        /// <summary>
        /// Writes to file on disk. Default is true.
        /// </summary>
        public bool WriteToFile { get; set; } = true;

        /// <summary>
        /// Appends date to log entry. Default is true.
        /// </summary>
        public bool AppendDates { get; set; } = true;
        
        /// <summary>
        /// Appends category to log entry. Default is true.
        /// </summary>
        public bool AppendCategory { get; set; } = true;
        
        /// <summary>
        /// If true, writes errors to System.Diagnostics.Debug, This is useful for getting logs if you're 
        /// stuck working in a sad IDE like Visual Studio that doesn't have a real terminal.
        /// Default is false.
        /// </summary>
        public bool WriteToDiagnostics { get; set; }

        /// <summary>
        /// Interval in milliseconds for writing log buffer contents to disk.
        /// </summary>
        public int WriteInterval { get; set; } = 1000;

        /// <summary>
        /// Strictest is 0, higher is more verbose. Default is 0.
        /// </summary>
        public int VerbosityThreshold { get; set; }
 
        /// <summary>
        /// For Trace/Debug, namespace strings to block from logging. Matched against beginning of 
        /// source namespace/string, egs "System." will block everything that starts with "System."
        /// </summary>
        public IEnumerable<string> SourceBlock { get; set; } = new List<string>();

        /// <summary>
        /// For Trace/Debug, namespace strings to allow for logging. Matched against beginning of 
        /// source namespace/string, egs "System." will allow only sources that start with "System."
        /// </summary>
        public IEnumerable<string> SourceAllow { get; set; } = new List<string>();

        public LogLevel LogLevel { get; set;} = LogLevel.Warn; 

        #endregion
        
        #region CTORS

        public Loggger(string path)
        {
            string baseName = Path.GetDirectoryName(path);
            string file = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            string day = this.ToIsoShort(DateTime.UtcNow);
            _path = Path.Join(baseName, $"{file}{day}{extension}");

            try 
            {
                if (!Directory.Exists(baseName))
                    Directory.CreateDirectory(baseName);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"ERROR : could not create log directory \"{baseName}\"");
                throw;
            }

            _timer = new System.Timers.Timer(this.WriteInterval); 
            _timer.Elapsed += this.FlushDebounce;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();
        }

        #endregion

        #region METHODS

        public void Error(object source, object arg)
        {
            Error(source, string.Empty, arg);
        }
        
        public void Error(object source, string message)
        {
            Error(source, message, null);
        } 

        /// <summary>
        /// Errors. Exactly what the name says. These should always be logged, there is no such thing as "critical" because 
        /// there's no such thing as a minor plane crash, it's just "plane crash". Cannot be disabled.
        /// </summary>
        public void Error(object sourceContext, string message, object exception) 
        {
            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message
                    .Replace("{", " ")
                    .Replace("}", " ");

            string source = GetTypeName(sourceContext);
            if (!string.IsNullOrEmpty(source))
                source = $"{source}|";

            string dateString = GenerateDateString();
            string category_lead = "ERR";
            string category = this.AppendCategory ? $"{category_lead}|" : string.Empty;
            string logLine = $"{category}{dateString}{source} {message}";

            if (this.WriteToConsole)
                Console.WriteLine(logLine, source);

            if (this.WriteToDiagnostics)
            {
                System.Diagnostics.Debug.WriteLine(logLine, source);
                if (exception != null)
                    System.Diagnostics.Debug.WriteLine(exception);
            }

            if (this.WriteToFile) 
                lock(_buffer)
                {
                    _buffer.Add(logLine);
                    if (exception != null)
                        _buffer.Add($"{dateString}{exception}");
                }
        }

        public void Warn(object source, string message) 
        {
            Warn(source, message, null);
        }

        public void Warn(object source, object exception) 
        {
            Warn(source, null, exception);
        }

        /// <summary>
        /// A bad thing that you're going to put off until it turns up in Error. Cannot be disabled.
        /// </summary>
        public void Warn(object sourceContext, string message, object exception)
        {
            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message
                    .Replace("{", " ")
                    .Replace("}", " ");

            string source = this.GetTypeName(sourceContext);
            if (!string.IsNullOrEmpty(source))
                source = $"{source}|";

            string dateString = GenerateDateString();
            string category_lead = "WRN";
            string category = this.AppendCategory ? $"{category_lead}|" : string.Empty;
            string logLine = $"{category}{dateString}{source} {message}";

            if (this.WriteToConsole)
                Console.WriteLine(logLine, source);

            if (this.WriteToDiagnostics)
                System.Diagnostics.Debug.WriteLine(logLine, source);

            if (exception != null)
                System.Diagnostics.Debug.WriteLine(exception);

            if (this.WriteToFile) 
                lock(_buffer)
                {
                    _buffer.Add(logLine);
                    if (exception != null)
                        _buffer.Add($"{dateString}{exception}");
                }
        }

        public void Status(object source, string message, int verbosity = 0)
        {
            Status(GetTypeName(source), message, verbosity);
        }

        /// <summary>
        /// Important things that have been done, and which must be logged for AUDITING purposes. 
        /// Tells us about normal operations. Has its own verbosity scale and is aimed at REGULAR
        /// users/admins, not developers. Verbosity starts at zero with zero being most important,
        /// 0 verbosity cannot be disabled.
        /// </summary>
        public void Status(string source, string message, int verbosity=0)
        {
            if (verbosity > this.VerbosityThreshold)
                return;

            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message
                    .Replace("{", " ")
                    .Replace("}", " ");

            if (!string.IsNullOrEmpty(source))
                source = $"{source}|";

            string dateString = GenerateDateString();
            string category_lead = "STA";
            string category = this.AppendCategory ? $"{category_lead}|" : string.Empty;
            string logLine = $"{category}{dateString}{source} {message}";

            if (this.WriteToConsole)
                Console.WriteLine(logLine, source);

            if (this.WriteToDiagnostics)
                System.Diagnostics.Debug.WriteLine(logLine, source);

            if (this.WriteToFile) 
                lock(_buffer)
                    _buffer.Add(logLine);
        }
        
        public void Debug(object source, string message, int verbosity = 0) 
        {
            this.Debug(GetTypeName(source), message, verbosity);
        }

        /// <summary>
        /// Things that have been done, tells us about normal operation. Verbosity 0 cannot be disabled.
        /// </summary>
        public void Debug(string source, string message, int verbosity = 0)
        {
            if (this.LogLevel > LogLevel.Debug)
                return;

            if (verbosity > this.VerbosityThreshold)
                return;

            if (!string.IsNullOrEmpty(source) && SourceAllow.Any() && !SourceAllow.Any(f => f == source))
                return;

            if (!string.IsNullOrEmpty(source) && SourceBlock.Any(f => f == source))
                return;

            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message
                    .Replace("{", " ")
                    .Replace("}", " ");

            if (!string.IsNullOrEmpty(source))
                source = $"{source}|";

            string dateString = GenerateDateString();
            string category_lead = "DBG";
            string category = this.AppendCategory ? $"{category_lead}|" : string.Empty;
            string logLine = $"{category}{dateString}{source} {message}";

            if (this.WriteToConsole)
                Console.WriteLine(logLine, source);

            if (this.WriteToDiagnostics)
                System.Diagnostics.Debug.WriteLine(logLine, source);

            if (this.WriteToFile) 
                lock(_buffer)
                    _buffer.Add(logLine);
        }

        public void Trace(object source, string message, int verbosity = 0)
        {
            this.Trace(GetTypeName(source), message);
        }

        public void Trace(string source, string message, int verbosity = 0)
        {
            if (this.LogLevel > LogLevel.Trace)
                return;

            if (verbosity > this.VerbosityThreshold)
                return;

            if (!string.IsNullOrEmpty(source) && SourceAllow.Any() && !SourceAllow.Any(f => f == source))
                return;

            if (!string.IsNullOrEmpty(source) && SourceBlock.Any(f => f == source))
                return;

            // strip out curly braces from messages, these will break console out on C#
            if (message != null)
                message = message
                    .Replace("{", " ")
                    .Replace("}", " ");

            if (!string.IsNullOrEmpty(source))
                source = $"{source}|";

            string dateString = GenerateDateString();
            string category_lead = "TRC";
            string category = this.AppendCategory ? $"{category_lead}|" : string.Empty;
            string logLine = $"{category}{dateString}{source} {message}";

            if (this.WriteToConsole)
                Console.WriteLine(logLine, source);

            if (this.WriteToDiagnostics)
                System.Diagnostics.Debug.WriteLine(logLine, source);

            if (this.WriteToFile) 
                lock(_buffer)
                    _buffer.Add(logLine);
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();

            this.Flush();
        }

        /// <summary>
        /// Writes buffer contents to file.
        /// </summary>
        private void Flush()
        {
            IEnumerable<string> block;

            // Don't thread lock this, we want as little thread locking as possible.
            // Worst thing that happens is we miss log entries for this
            // pass of timer, we'll get that on next pass. 
            if (!_buffer.Any())
                return;

            // Copy everything from buffer to temporary write block, then release thread.
            // Once again, we want to lock for as few operations as possible.
            lock(_buffer)
            {
                block = _buffer.ToArray();
                _buffer.Clear();
            }

            // store in field for added resilience. If a few writes to log fail for some
            // or other reason, we keep log entries in write buffer until they can be written.
            _writeBuffer.AddRange(block);

            try 
            {
                // This can most likely be improved upon, but it's good enough. A file lock
                // would be nice, but linux doesn't support that.
                File.AppendAllLines(_path, _writeBuffer);

                _writeBuffer.Clear();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"ERROR : Write to log file \"{_path}\" failed : {ex}");
            }
            finally
            {
                // this should never happen, but in the event the log file is permanently broken
                // to writes, we don't want to eat unlimited memory with write buffer. 
                if (_writeBuffer.Count() > 10000)
                {
                    _writeBuffer.Clear();
                    Console.WriteLine("ERROR : Write to log in permanent broken state, write buffer permanently lost");
                }
            }
        }

        /// <summary>
        /// Call from timer.Elapsed only. Ensures that timed calls don't collide in the event that Flush() 
        /// blocks across events.
        /// </summary>
        private void FlushDebounce(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (_writing)
                return;
            
            try 
            {
                _writing = true;
                this.Flush();
            }
            finally
            {
                _writing = false;
            }
        }

        /// <summary>
        /// Converts to yyyy-mm-dd ISO
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private string ToIsoShort(DateTime date)
        {
            string iso = date
                .ToLocalTime()
                .ToString("s")      // convert to ymdhms
                .Replace("T", " "); // replace T after ymd

            return iso
                .Substring(0, iso.Length - 9); // remove all time data
        }

        private string GenerateDateString() 
        {
            return this.AppendDates ? $"{DateTime.UtcNow.ToString(this.DateFormat)}|" : string.Empty;
        }

        private string GetTypeName(object obj)
        {
            return GetTypeName(obj.GetType());
        }

        private string GetTypeName(Type type)
        {
            string name = $"{type.Namespace}.{type.Name}";
            if (name.EndsWith("`1"))
                name = name.Substring(0, name.Length - 2);

            return name;
        }

        #endregion
    }
}


//PORTER-WRAPPER!
}
//PORTER-WRAPPER!