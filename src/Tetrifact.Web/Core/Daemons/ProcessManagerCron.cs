using Tetrifact.Core.Porter_Packages.Madscience.Loggger;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ProcessManagerCron : Cron
    {
        #region FIELDS

        private readonly ILoggger _log;

        private readonly IProcessManagerFactory _processManagerFactory;

        private readonly IDaemon _daemonrunner;

        private readonly ISettings _settings;

        #endregion

        #region CTORS

        public ProcessManagerCron(
            ISettings settings, 
            IDaemon daemonrunner, 
            IProcessManagerFactory processManagerFactory, 
            ILoggger log)
        {
            _settings = settings;
            _processManagerFactory = processManagerFactory;
            _log = log;
            _daemonrunner = daemonrunner;
        }

        #endregion

        #region METHODS

        public override void Start()
        {
            if (string.IsNullOrEmpty(_settings.CleanCronMask))
                _log.Status(this, "Clean mask empty, cleaner daemon disabled.");
            else
            {
                _log.Status(this, "Starting cleaner daemon");
                _daemonrunner.Start(1000, new DaemonWorkMethod(this.Work));
            }
        }

        public override void Stop()
        {
            _daemonrunner.Stop();
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        public override async Task Work()
        {
            _processManagerFactory.ClearExpired();
        }

        #endregion
    }
}
