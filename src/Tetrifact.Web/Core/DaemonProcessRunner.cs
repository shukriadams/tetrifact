using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Tetrifact.Web
{
    /// <summary>
    /// Keep-alive logic for daemon, kept in separate class to excluded it from unit testing.
    /// </summary>
    public class DaemonProcessRunner : IDaemonProcessRunner
    {
        private bool _running;
        
        private bool _busy;

        private ILogger<IDaemonProcessRunner> _log;

        public DaemonProcessRunner(ILogger<IDaemonProcessRunner> log)
        { 
            _log = log;
            _running = true;
        }

        public void Start(DaemonWork work, int tickInterval)
        {
            Task.Run(() => {
                while (_running)
                {
                    try
                    {
                        _log.LogInformation("Daemon ticked");

                        if (_busy)
                            return;

                        _busy = true;

                        work();
                    }
                    finally
                    {
                        _busy = false;
                        Thread.Sleep(tickInterval);
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
