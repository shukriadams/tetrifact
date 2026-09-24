using Tetrifact.Core.Porter_Packages.Madscience.Loggger;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ArchiveGenerator : Cron
    {
        #region FIELDS

        private readonly ILoggger _log;
        
        private readonly IDaemon _daemonrunner;

        private readonly IArchiveService _archiveService;


        #endregion

        #region CTORS

        public ArchiveGenerator(
            IDaemon daemonrunner, 
            IArchiveService archiveService, 
            ILoggger log)
        {
            _archiveService = archiveService;
            _daemonrunner = daemonrunner;
            _log = log;
        }

        #endregion

        #region METHODS

        public override void Start()
        {
            _log.Status(this, "Starting archive generating daemon");
            _daemonrunner.Start(1000, new DaemonWorkMethod(this.Work));
        }

        public override void Stop()
        {
            _daemonrunner.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        public override async Task Work()
        {
            await _archiveService.CreateNextQueuedArchive();
        }

        #endregion
    }
}
