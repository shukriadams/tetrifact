using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class Daemon
    {
        private int _tickInterval;
        private readonly IRepositoryCleaner _repositoryCleaner;
        private readonly IIndexReader _indexService;
        private bool _busy;
        private bool _running;
        private ILogger<Daemon> _log;

        public Daemon(IRepositoryCleaner repositoryCleaner, IIndexReader indexService, ILogger<Daemon> log)
        {
            _indexService = indexService;
            _repositoryCleaner = repositoryCleaner;
            _log = log;
        }

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
                    _log.LogInformation("Daemon ticking");


                    if (_busy)
                        return;

                    _busy = true;

                    _repositoryCleaner.Clean();
                    _indexService.PurgeOldArchives();
                }
                finally
                {
                    _busy = false;
                    await Task.Delay(_tickInterval);
                }
            }
        }
    }
}
