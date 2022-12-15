using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Tetrifact.Web
{
    /// <summary>
    /// Runs Daemon on its own task thread. We keep this separate from daemon logic for easier unit testing.
    /// </summary>
    public class DaemonBackgroundProcess : IDaemonBackgroundProcess
    {
        private bool _running;
        
        private bool _busy;

        private ILogger<IDaemonBackgroundProcess> _log;

        public DaemonBackgroundProcess(ILogger<IDaemonBackgroundProcess> log)
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
