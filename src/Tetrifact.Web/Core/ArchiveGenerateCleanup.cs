using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ArchiveGenerateCleanup : Cron
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<ArchiveGenerateCleanup> _log;

        private readonly IDaemon _daemonrunner;

        private readonly IFileSystem _fileSystem;

        private readonly IMemoryCache _cache;

        private readonly IArchiveService _archiveService;

        #endregion

        #region CTORS

        public ArchiveGenerateCleanup(IDaemon daemonrunner, IArchiveService archiveService, IMemoryCache cache, IFileSystem fileSystem, ILogger<ArchiveGenerateCleanup> log)
        {
            _settings = new Settings();
            _fileSystem = fileSystem;
            _cache = cache;
            _daemonrunner = daemonrunner;
            _archiveService = archiveService;
            _log = log;
        }

        #endregion

        #region METHODS

        public override void Start()
        {
            _daemonrunner.Start(1000, new DaemonWorkMethod(this.Work));
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Work()
        {
            _archiveService.CleanupNextQueuedArchive();
        }

        #endregion
    }
}
