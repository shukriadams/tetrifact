using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;

namespace Tetrifact.Web
{
    /// <summary>
    /// Runs Daemon on its own task thread. We keep this separate from daemon logic for easier unit testing.
    /// </summary>
    public class Daemon : IDaemon, IDisposable
    {
        private bool _running;
        
        private bool _busy;

        private DateTime _lastRun;

        private CronExpression _cronExpression;

        public Daemon()
        {
            _running = true;
        }

        public void Start(Cron daemon)
        {
            _cronExpression = CronExpression.Parse(daemon.CronMask);
            _lastRun = DateTime.UtcNow;

            Task.Run(() => {
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
                        daemon.Work();
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                        _busy = false;
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _running = false;
        }
    }
}
