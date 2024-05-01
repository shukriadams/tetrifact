using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class ArchiveStatusChecker : Cron
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<ArchiveStatusChecker> _log;

        private readonly IDaemon _daemonrunner;

        private readonly IArchiveService _archiveService;

        private readonly IFileSystem _fileSystem;

        private readonly IMemoryCache _cache;

        private IIndexReadService _indexReader;

        #endregion

        #region CTORS

        public ArchiveStatusChecker(IDaemon daemonrunner, IIndexReadService indexReader, IMemoryCache cache, IFileSystem fileSystem, IArchiveService archiveService, ILogger<ArchiveStatusChecker> log)
        {
            _settings = new Settings();
            _archiveService = archiveService;
            _fileSystem = fileSystem;
            _daemonrunner = daemonrunner;
            _indexReader = indexReader;
            _log = log;
            _cache = cache;
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
                }
                catch (Exception ex)
                {
                    _log.LogError($"Error generating archive from queue file {queueFile}", ex);
                    continue;
                }

                string progressKey = _archiveService.GetArchiveProgressKey(archiveQueueInfo.PackageId);
                ArchiveProgressInfo progress = _cache.Get<ArchiveProgressInfo>(progressKey);

                if (progress == null)
                {
                    Manifest manifest = _indexReader.GetManifest(archiveQueueInfo.PackageId);

                    // hardcode the compression factor, this needs its own calculation routine
                    double compressionFactor = 0.6;
                    progress = new ArchiveProgressInfo
                    {
                        PackageId = archiveQueueInfo.PackageId,
                        ProjectedSize = (long)Math.Round(manifest.Size * compressionFactor),
                        State = PackageArchiveCreationStates.Queued,
                        QueuedUtc = archiveQueueInfo.QueuedUtc,
                    };
                }

                _cache.Set(progressKey, progress);

                // attemtpt to calculate compression progress by measuring size of temp zip on disk
                string archiveTempPath = _archiveService.GetPackageArchiveTempPath(archiveQueueInfo.PackageId);
                FileInfo tempArchiveFileInfo;
                decimal compressionPercentDone = 0;

                if (_fileSystem.File.Exists(archiveTempPath))
                {
                    long length;
                    try
                    {
                        tempArchiveFileInfo = new FileInfo(archiveTempPath);
                        length = tempArchiveFileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning($"Could not read file info for temp-state archive {archiveTempPath}", ex);
                        // ignore error if w
                        continue;
                    }

                    
                    if (progress.ProjectedSize != 0)
                        compressionPercentDone = 100 * ((decimal)length / (decimal)progress.ProjectedSize);
                }

                ArchiveProgressInfo cachedProgress = _cache.Get<ArchiveProgressInfo>(progressKey);
                cachedProgress.CompressProgress = compressionPercentDone;
                cachedProgress.CombinedPercent = (cachedProgress.CompressProgress + cachedProgress.FileCopyProgress) / 2;

                _cache.Set(progressKey, cachedProgress);
            }
            // 
        }

        #endregion
    }
}
