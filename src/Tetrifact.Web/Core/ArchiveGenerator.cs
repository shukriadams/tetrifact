using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ArchiveGenerator : Cron
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<ArchiveGenerator> _log;
        
        private readonly IDaemon _daemonrunner;

        private readonly IArchiveService _archiveService;

        #endregion

        #region CTORS

        public ArchiveGenerator(IDaemon daemonrunner, IArchiveService archiveService, ILogger<ArchiveGenerator> log)
        {
            _settings = new Settings();
            _archiveService = archiveService;
            this.CronMask = "* * * * *"; // 1 minute, move to settings?
            _log = log;
        }

        #endregion

        #region METHODS

        public override void Start()
        {
            _daemonrunner.Start(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Work()
        {
            // process flags in series, as we assume disk cannot handle more than one archive at a time

            string packageId= "ready from flag";
            // 
            _archiveService.CreateArchive(packageId);
        }

        #endregion
    }
}
