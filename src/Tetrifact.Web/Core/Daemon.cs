using Microsoft.Extensions.Logging;
using System;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public delegate void DaemonWork();

    /// <summary>
    /// Internal timed process that manages automated processes, such as cleanup, integrity checks, etc.
    /// </summary>
    public class Daemon : IDaemon 
    {
        #region FIELDS

        private readonly IRepositoryCleanService _repositoryCleaner;

        private readonly IArchiveService _archiveService;

        private readonly ILogger<IDaemon> _log;

        private readonly IPackagePruneService _packagePrune;

        private readonly IDaemonProcessRunner _processRunner;

        #endregion

        #region CTORS

        public Daemon(IRepositoryCleanService repositoryCleaner, IArchiveService archiveService, IDaemonProcessRunner processRunner, IPackagePruneService packagePrune, ILogger<IDaemon> log)
        {
            _packagePrune = packagePrune;
            _archiveService = archiveService;
            _repositoryCleaner = repositoryCleaner;
            _processRunner = processRunner;
            _log = log;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Replaces the manual constructor in that we can pass the interval to this.
        /// </summary>
        /// <param name="tickInterval"></param>
        public void Start(int tickInterval)
        {
            _processRunner.Start(new DaemonWork(this.Work), tickInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        { 
            _processRunner.Dispose();
        }

        /// <summary>
        /// Daemon's main work method
        /// </summary>
        private void Work()
        { 
            try 
            {
                _repositoryCleaner.Clean();
            }
            catch (Exception ex)
            {
                _log.LogError("Daemon repository clean error", ex);
            }

            try
            {
                _archiveService.PurgeOldArchives();
            }
            catch (Exception ex)
            {
                _log.LogError("Daemon Purge archives error", ex);
            }

            try
            {
                _packagePrune.Prune();
            }
            catch (Exception ex)
            {
                _log.LogError("Daemon prune error", ex);
            }
        }

        #endregion
    }
}
