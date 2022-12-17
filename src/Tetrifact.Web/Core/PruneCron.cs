using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class PruneCron : Cron 
    {
        private readonly ILogger<PruneCron> _log;

        private readonly IPackagePruneService _packagePrune;

        private readonly IDaemon _daemonrunner;

        public PruneCron(IPackagePruneService packagePrune, IDaemon daemonrunner, ILogger<PruneCron> log)
        {
            Settings s = new Settings();
            this.CronMask = s.PruneCronMask;

            _log = log;
            _packagePrune = packagePrune;
            _daemonrunner = daemonrunner;
        }

        public override void Start() 
        {
            _daemonrunner.Start(this);
        }        

        public override void Work()
        {
            try
            {
                _packagePrune.Prune();
            }
            catch (Exception ex)
            {
                _log.LogError("Daemon prune error", ex);
            }
        }
    }
}
