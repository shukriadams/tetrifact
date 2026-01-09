using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tetrifact.Core;
using Tetrifact.Web.Porter_Packages.MadScience_SimpleDI;

namespace Tetrifact.Web
{
    public class PruneCron : Cron 
    {
        private readonly ILogger<PruneCron> _log;

        private readonly IDaemon _daemonrunner;

        private readonly ISettings _settings;

        private readonly IPackageListCache _packageListCache;
    
        public PruneCron(ISettings settings, IPackageListCache packageListCache, IDaemon daemonrunner, ILogger<PruneCron> log)
        {
            _settings = settings;
            _log = log;
            _daemonrunner = daemonrunner;
            _packageListCache = packageListCache;
        }

        public override void Start() 
        {
            if (string.IsNullOrEmpty(_settings.PruneCronMask))
                _log.LogInformation("Prune mask empty, prune daemon disabled.");
            else
            {
                _log.LogInformation("Starting prune daemon");
                _daemonrunner.Start(_settings.PruneCronMask, new DaemonWorkMethod(this.Work));
            }
        }

        public override async Task Work()
        {
            try
            {
                _log.LogInformation("Starting prune from daemon");
                SimpleDI di = new SimpleDI();
                IPruneService pruneService = di.Resolve<IPruneService>();
                pruneService.Prune();
                _packageListCache.Clear();
            }
            catch (Exception ex)
            {
                _log.LogError($"Daemon prune error {ex}");
            }
        }
    }
}
