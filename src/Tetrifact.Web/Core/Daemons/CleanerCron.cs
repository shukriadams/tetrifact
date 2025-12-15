using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    /// <summary>
    /// Internal timed process that manages automated processes, such as cleanup, integrity checks, etc.
    /// </summary>
    public class CleanerCron : Cron
    {
        #region FIELDS

        private readonly IServiceProvider _serviceProvider;

        private readonly IArchiveService _archiveService;

        private readonly ILogger<CleanerCron> _log;
        
        private readonly IDaemon _daemonrunner;

        private readonly ISettings _settings;

        #endregion

        #region CTORS

        public CleanerCron(IServiceProvider serviceProvider, ISettings settings, IDaemon daemonrunner, IArchiveService archiveService, ILogger<CleanerCron> log)
        {
            _settings = settings;

            _archiveService = archiveService;
            _serviceProvider = serviceProvider;
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
                _daemonrunner.Start(_settings.CleanCronMask, new DaemonWorkMethod(this.Work));
            }
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        public override async Task Work()
        {
            try
            {
                _log.LogInformation("Starting clean from daemon");
                IRepositoryCleanService cleanService = _serviceProvider.GetService<IRepositoryCleanService>();
                cleanService.Clean();
            }
            catch (Exception ex)
            {
                _log.LogError($"Daemon repository clean error {ex}");
            }

            try
            {
                _archiveService.PurgeOldArchives();
            }
            catch (Exception ex)
            {
                _log.LogError($"Daemon Purge archives error {ex}");
            }

        }

        #endregion
    }
}
