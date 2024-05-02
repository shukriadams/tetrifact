using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
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
            // process flags in series, as we assume disk cannot handle more than one archive at a time
            string[] queueFiles = _fileSystem.Directory.GetFiles(_settings.ArchiveQueuePath);
            foreach (string queueFile in queueFiles)
            {
                ArchiveQueueInfo archiveQueueInfo;
                try
                {
                    string queueFileContent = _fileSystem.File.ReadAllText(queueFile);
                    archiveQueueInfo = JsonConvert.DeserializeObject<ArchiveQueueInfo>(queueFileContent);
                    string key = _archiveService.GetArchiveProgressKey(archiveQueueInfo.PackageId);
                    ArchiveProgressInfo progress = _cache.Get<ArchiveProgressInfo>(key);

                    if (progress == null || progress.State != PackageArchiveCreationStates.Processed_CleanupRequired)
                        continue;

                    string tempDir2 = Path.Join(_settings.TempPath, $"_repack_{archiveQueueInfo.PackageId}");
                    if (_fileSystem.Directory.Exists(tempDir2))
                        _fileSystem.Directory.Delete(tempDir2, true);

                    _fileSystem.File.Delete(queueFile);
                    _cache.Remove(key);
                }
                catch (Exception ex)
                {
                    _log.LogError($"Error cleaning up archive from queue file {queueFile} {ex}");
                    continue;
                }
            }
        }

        #endregion
    }
}
