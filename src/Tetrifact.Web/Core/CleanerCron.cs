﻿using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    /// <summary>
    /// Internal timed process that manages automated processes, such as cleanup, integrity checks, etc.
    /// </summary>
    public class CleanerCron : Cron
    {
        #region FIELDS

        private readonly IRepositoryCleanService _repositoryCleaner;

        private readonly IArchiveService _archiveService;

        private readonly ILogger<CleanerCron> _log;

        private readonly ILock _lock;
        
        private readonly IDaemon _daemonrunner;

        private readonly ISettings _settings;

        #endregion

        #region CTORS

        public CleanerCron(IRepositoryCleanService repositoryCleaner, IDaemon daemonrunner, IArchiveService archiveService, ILock lockInstance, ILogger<CleanerCron> log)
        {
            _settings = new Settings();
            this.CronMask = _settings.CleanCronMask;

            _archiveService = archiveService;
            _repositoryCleaner = repositoryCleaner;
            _lock = lockInstance;
            _log = log;
            _daemonrunner = daemonrunner;
        }

        #endregion

        #region METHODS

        public override void Start()
        {
            _daemonrunner.Start(this);
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        public override void Work()
        {
            try
            {
                _repositoryCleaner.Clean();
            }
            catch (Exception ex)
            {
                _log.LogError($"Daemon repository clean error {ex}");
            }

            try
            {
                _lock.ClearExpired();
            }
            catch (Exception ex)
            {
                _log.LogError($"Daemon lock clear error {ex}");
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
