using System;
using System.Threading;
using Cronos;
using Microsoft.Extensions.Logging;

namespace Tetrifact.Web
{
    /// <summary>
    /// Runs Daemon on its own task thread. We keep this separate from daemon logic for easier unit testing.
    /// </summary>
    public class Daemon : IDaemon
    {
        private bool _running;
        
        private bool _busy;

        private CronExpression _cronExpression;

        private DateTime _lastRun;

        private ILogger<Daemon> _log;

        public Daemon(ILogger<Daemon> log)
        {
            _log = log;
            _running = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval">milliseconds</param>
        public void Start(int interval, DaemonWorkMethod work)
        {
            new Thread(async delegate ()
            {
                while (_running)
                {
                    try
                    {
                        if (_busy)
                            continue;

                        _busy = true;
                        await work();
                    }
                    catch(Exception ex)
                    { 
                        _log.LogError(ex, $"Unhandled daemon exception from {work.Method.DeclaringType.Name}");
                    }
                    finally
                    {
                        Thread.Sleep(interval);
                        _busy = false;
                    }
                }
            }).Start();
        }

        public void Start(string cronmask, DaemonWorkMethod work)
        {
            _cronExpression = CronExpression.Parse(cronmask);
            _lastRun = DateTime.UtcNow;

            new Thread(async delegate ()
            {
                while (_running)
                {
                    try
                    {
                        if (_busy)
                            continue;

                        DateTime? nextUtc = _cronExpression.GetNextOccurrence(_lastRun);
                        if (nextUtc > DateTime.UtcNow)
                            continue;

                        _busy = true;
                        _lastRun = DateTime.UtcNow;
                        await work();
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, $"Unhandled daemon exception from {work.Method.DeclaringType.Name}");
                    }
                    finally
                    {
                        _busy = false;
                        Thread.Sleep(60000); // recheck cron tick every minute, no need to check more frequently given minute resolution of cronmask
                    }
                }
            }).Start();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            _running = false;
        }
    }
}
