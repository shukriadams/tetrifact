using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class MetricsCron : Cron
    {
        private ILogger<MetricsCron> _log;

        private IMetricsService _metricsService;

        private IHostApplicationLifetime _applicationLifetime;
        
        private readonly IDaemon _daemonrunner;

        public MetricsCron(IMetricsService metricsService, IDaemon daemonrunner, IHostApplicationLifetime applicationLifetime, ILogger<MetricsCron> log) 
        {
            Settings s = new Settings();
            this.CronMask = s.MetricsCronMask;

            _log = log;
            _metricsService = metricsService;
            _applicationLifetime = applicationLifetime;
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
                _metricsService.Generate();
            }
            catch (FatalException ex)
            {
                // error has already been logged, go straight to shutdown
                _log.LogError("Fatal error - failed to delete corrupt last_run file : ", ex);
                _applicationLifetime.StopApplication();
            }
            catch (Exception ex)
            {
                _log.LogError("Daemon metrics generate error", ex);
            }
        }
    }
}
