using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    /// <summary>
    /// Internal timed process that manages automated processes, such as cleanup, integrity checks, etc.
    /// </summary>
    public class Daemon
    {
        #region FIELDS

        private int _tickInterval;

        private readonly IRepositoryCleanService _repositoryCleaner;

        private readonly IIndexReadService _indexService;

        private readonly IArchiveService _archiveService;

        private bool _busy;

        private bool _running;

        private ILogger<Daemon> _log;

        private IPackagePruneService _packagePrune;

        #endregion

        #region CTORS

        public Daemon(IRepositoryCleanService repositoryCleaner, IIndexReadService indexService, IArchiveService archiveService, IPackagePruneService packagePrune, ILogger<Daemon> log)
        {
            _indexService = indexService;
            _packagePrune = packagePrune;
            _archiveService = archiveService;
            _repositoryCleaner = repositoryCleaner;
            _log = log;
        }

        #endregion

        #region METHODS

        public void Start(int tickInterval)
        {
            _tickInterval = tickInterval;
            Task.Run(async () => this.Tick());
            _running = true;

        }

        public void Stop(){
            _running = false;
        }

        private async Task Tick()
        { 
            while(_running){
                try
                {
                    _log.LogInformation("Daemon ticked");


                    if (_busy)
                        return;

                    _busy = true;

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
                finally
                {
                    _busy = false;
                    await Task.Delay(_tickInterval);
                }
            }
        }

        #endregion
    }
}
