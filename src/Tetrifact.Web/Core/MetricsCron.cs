using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class MetricsCron : Cron
    {
        private ILogger<MetricsCron> _log;

        private IMetricsService _metricsService;

        private IHostApplicationLifetime _applicationLifetime;
        
        private readonly IDaemon _daemonrunner;

        private readonly Settings _settings;

        public MetricsCron(IMetricsService metricsService, IDaemon daemonrunner, IHostApplicationLifetime applicationLifetime, ILogger<MetricsCron> log) 
        {
            _settings = new Settings();
            _log = log;
            _metricsService = metricsService;
            _applicationLifetime = applicationLifetime;
            _daemonrunner = daemonrunner;
        }

        public override void Start()
        {
            if (string.IsNullOrEmpty(_settings.MetricsCronMask))
                _log.LogInformation("Metrics mask empty, metrics daemon disabled.");
            else
            {
                _log.LogInformation("Starting metrics daemon");
                _daemonrunner.Start(_settings.MetricsCronMask, new DaemonWorkMethod(this.Work));
            }
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
                _log.LogError($"Fatal error - failed to delete corrupt last_run file : {ex}");
                _applicationLifetime.StopApplication();
            }
            catch (Exception ex)
            {
                _log.LogError($"Daemon metrics generate error {ex}");
            }
        }
    }
}
