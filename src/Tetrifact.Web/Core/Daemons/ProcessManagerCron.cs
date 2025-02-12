using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ProcessManagerCron : Cron
    {
        #region FIELDS

        private readonly ILogger<CleanerCron> _log;

        private readonly IProcessManager _lock;

        private readonly IDaemon _daemonrunner;

        private readonly ISettings _settings;

        #endregion

        #region CTORS

        public ProcessManagerCron(ISettings settings, IDaemon daemonrunner, IProcessManager lockInstance, ILogger<CleanerCron> log)
        {
            _settings = settings;
            _lock = lockInstance;
            _log = log;
            _daemonrunner = daemonrunner;
        }

        #endregion

        #region METHODS

        public override void Start()
        {
            if (string.IsNullOrEmpty(_settings.CleanCronMask))
                _log.LogInformation("Clean mask empty, cleaner daemon disabled.");
            else
            {
                _log.LogInformation("Starting cleaner daemon");
                _daemonrunner.Start(1000, new DaemonWorkMethod(this.Work));
            }
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        public override async Task Work()
        {
            _lock.ClearExpired();
        }

        #endregion
    }
}
