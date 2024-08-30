using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ArchiveGenerateCleanupCron : Cron
    {
        #region FIELDS

        private readonly IDaemon _daemonrunner;

        private readonly IArchiveService _archiveService;

        ILogger<ArchiveGenerateCleanupCron> _log;

        #endregion

        #region CTORS

        public ArchiveGenerateCleanupCron(IDaemon daemonrunner, IArchiveService archiveService, ILogger<ArchiveGenerateCleanupCron> log)
        {
            _daemonrunner = daemonrunner;
            _archiveService = archiveService;
            _log = log;
        }

        #endregion

        #region METHODS

        public override void Start()
        {
            _log.LogInformation("Starting archive generating cleanup daemon");
            _daemonrunner.Start(1000, new DaemonWorkMethod(this.Work));
        }

        /// <summary>
        /// 
        /// </summary>
        public override async Task Work()
        {
            _archiveService.CleanupNextQueuedArchive();
        }

        #endregion
    }
}