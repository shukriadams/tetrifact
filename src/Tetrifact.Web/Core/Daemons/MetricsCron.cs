using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Tetrifact.Core;
using Tetrifact.Core.Porter_Packages.Madscience.Loggger;

namespace Tetrifact.Web
{
    public class MetricsCron : Cron
    {
        #region FIELDS

        private ILoggger _log;

        private IMetricsService _metricsService;
        
        private readonly IDaemon _daemonrunner;

        private readonly ISettings _settings;

        #endregion

        #region CTORS

        public MetricsCron(
            IMetricsService metricsService, 
            ISettings settings, 
            IDaemon daemonrunner, 
            ILoggger log) 
        {
            _settings = settings;
            _log = log;
            _metricsService = metricsService;
            _daemonrunner = daemonrunner;
        }

        #endregion

        #region METHODS

        public override void Start()
        {
            if (string.IsNullOrEmpty(_settings.MetricsCronMask))
                _log.Status(this, "Metrics mask empty, metrics daemon disabled.");
            else
            {
                _log.Status(this, "Starting metrics daemon");
                _daemonrunner.Start(_settings.MetricsCronMask, new DaemonWorkMethod(this.Work));
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
                _metricsService.Generate();
            }
            catch (FatalException ex)
            {
                // error has already been logged, go straight to shutdown
                _log.Error(this, $"Fatal error - failed to delete corrupt last_run file", ex);
  }
            catch (Exception ex)
            {
                _log.Error(this, $"Daemon metrics generated error", ex);
            }
        }

        #endregion
    }
}
