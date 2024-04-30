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

        #endregion

        #region CTORS

        public ArchiveGenerator(IDaemon daemonrunner, IFileSystem fileSystem, IArchiveService archiveService, ILogger<ArchiveGenerator> log)
        {
            _settings = new Settings();
            _archiveService = archiveService;
            _fileSystem = fileSystem;
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
                try 
                {
                    string queueFileContent = _fileSystem.File.ReadAllText(file);
                    ArchiveQueueInfo archiveQueueInfo = JsonConvert.DeserializeObject<ArchiveQueueInfo>(queueFileContent);
                    _archiveService.CreateArchive(archiveQueueInfo.PackageId);
                }
                catch (Exception ex)
                {
                    _log.LogError($"Error generating archive from queue file {file}", ex);
                }
                finally
                { 
                    try 
                    {
                        // always delete queue file after processing
                        _fileSystem.File.Delete(file);
                    }
                    catch (Exception ex)
                    { 
                        _log.LogError($"Error deleting archive queue file {file}", ex);
                    }
                }
            }

            // 
        }

        #endregion
    }
}
