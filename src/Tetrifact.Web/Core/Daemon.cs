using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    /// <summary>
    /// Internal timed process that manages automated processes, such as cleanup, integrity checks, etc.
    /// </summary>
    public class Daemon : IDisposable
    {
        #region FIELDS

        private int _tickInterval;

        private readonly IRepositoryCleanService _repositoryCleaner;

        private readonly IArchiveService _archiveService;

        private bool _busy;

        private bool _running;

        private ILogger<Daemon> _log;

        private IPackagePruneService _packagePrune;

        #endregion

        #region CTORS

        public Daemon(IRepositoryCleanService repositoryCleaner, IArchiveService archiveService, IPackagePruneService packagePrune, ILogger<Daemon> log)
        {
            _packagePrune = packagePrune;
            _archiveService = archiveService;
            _repositoryCleaner = repositoryCleaner;
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
            _tickInterval = tickInterval;

            Task.Run(() =>{
                while (_running)
                {
                    try
                    {
                        _log.LogInformation("Daemon ticked");

                        if (_busy)
                            return;

                        _busy = true;

                        this.Work();
                    }
                    finally
                    {
                        _busy = false;
                        Thread.Sleep(_tickInterval);
                    }
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _running = false;
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
