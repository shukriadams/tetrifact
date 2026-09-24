using System;
using System.Threading.Tasks;
using Tetrifact.Core;
using Tetrifact.Web.Porter_Packages.MadScience_SimpleDI;
using Tetrifact.Core.Porter_Packages.Madscience.Loggger;

namespace Tetrifact.Web
{
    public class PruneCron : Cron 
    {
        private readonly ILoggger _log;

        private readonly IDaemon _daemonrunner;

        private readonly ISettings _settings;

        private readonly IPackageListCache _packageListCache;
    
        public PruneCron(
            ISettings settings, 
            IPackageListCache packageListCache, 
            IDaemon daemonrunner, 
            ILoggger log)
        {
            _settings = settings;
            _log = log;
            _daemonrunner = daemonrunner;
            _packageListCache = packageListCache;
        }

        public override void Start() 
        {
            if (string.IsNullOrEmpty(_settings.PruneCronMask))
                _log.Status(this, "Prune mask empty, prune daemon disabled.");
            else
            {
                _log.Status(this, "Starting prune daemon");
                _daemonrunner.Start(_settings.PruneCronMask, new DaemonWorkMethod(this.Work));
            }
        }

        public override void Stop()
        {
            _daemonrunner.Stop();
        }

        public override async Task Work()
        {
            try
            {
                _log.Status(this, "Starting prune from daemon");
                SimpleDI di = new SimpleDI();
                IPruneService pruneService = di.Resolve<IPruneService>();
                pruneService.Prune();
                _packageListCache.Clear();
            }
            catch (Exception ex)
            {
                _log.Error(this, $"Daemon prune error", ex);
            }
        }
    }
}
