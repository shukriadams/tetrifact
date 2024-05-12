using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
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

        private readonly IFileSystem _fileSystem;

        private readonly IMemoryCache _cache;

        #endregion

        #region CTORS

        public ArchiveGenerator(IDaemon daemonrunner, IMemoryCache cache, IFileSystem fileSystem, IArchiveService archiveService, ILogger<ArchiveGenerator> log)
        {
            _settings = new Settings();
            _archiveService = archiveService;
            _fileSystem = fileSystem;
            _cache = cache;
            _daemonrunner = daemonrunner;
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
            _archiveService.CreateNextQueuedArchive();
        }

        #endregion
    }
}
