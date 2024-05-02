using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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
            // process flags in series, as we assume disk cannot handle more than one archive at a time
            string[] files = _fileSystem.Directory.GetFiles(_settings.ArchiveQueuePath);
            foreach(string file in files)
            {
                ArchiveQueueInfo archiveQueueInfo = null;
                ArchiveProgressInfo progress = null;
                string key = null;
                try 
                {
                    string queueFileContent = _fileSystem.File.ReadAllText(file);
                    archiveQueueInfo = JsonConvert.DeserializeObject<ArchiveQueueInfo>(queueFileContent);
                    key = _archiveService.GetArchiveProgressKey(archiveQueueInfo.PackageId);
                    progress = _cache.Get<ArchiveProgressInfo>(key);
                    if (progress == null || progress.State != PackageArchiveCreationStates.Queued)
                        continue;

                    progress.State = PackageArchiveCreationStates.ArchiveGenerating;
                    progress.StartedUtc = DateTime.UtcNow;
                    _cache.Set(key, progress);
                    _archiveService.CreateArchive(archiveQueueInfo.PackageId);
                }
                catch (Exception ex)
                {
                    _log.LogError($"Error generating archive from queue file {file} : {ex}");
                }
                finally
                { 
                    try 
                    {
                        // always mark for cleanup, even if fail
                        if (progress != null)
                        {
                            progress.State = PackageArchiveCreationStates.Processed_CleanupRequired;
                            _cache.Set(key, progress);
                        }
                    }
                    catch (Exception ex)
                    { 
                        _log.LogError($"Error updating archive queue file {file} : {ex}");
                    }
                }
            }
        }

        #endregion
    }
}
