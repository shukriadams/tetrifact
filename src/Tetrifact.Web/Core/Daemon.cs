using Microsoft.Extensions.Logging;
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
        private readonly IRepositoryCleaner _repositoryCleaner;
        private readonly IIndexReader _indexService;
        private bool _busy;
        private bool _running;
        private ILogger<Daemon> _log;
        private IPackagePrune _packagePrune;

        #endregion

        #region CTORS

        public Daemon(IRepositoryCleaner repositoryCleaner, IIndexReader indexService, IPackagePrune packagePrune, ILogger<Daemon> log)
        {
            _indexService = indexService;
            _packagePrune = packagePrune;
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

                    _repositoryCleaner.Clean();
                    _indexService.PurgeOldArchives();
                    _packagePrune.Prune();
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
