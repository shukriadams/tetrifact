using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class PruneCron : Cron 
    {
        private readonly ILogger<PruneCron> _log;

        private readonly IPackagePruneService _packagePrune;

        private readonly IDaemon _daemonrunner;

        private readonly Settings _settings;

        public PruneCron(IPackagePruneService packagePrune, IDaemon daemonrunner, ILogger<PruneCron> log)
        {
            _settings = new Settings();
            _log = log;
            _packagePrune = packagePrune;
            _daemonrunner = daemonrunner;
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
                _packagePrune.Prune();
            }
            catch (Exception ex)
            {
                _log.LogError($"Daemon prune error {ex}");
            }
        }
    }
}
