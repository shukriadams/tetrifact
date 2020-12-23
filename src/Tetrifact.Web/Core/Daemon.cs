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

        public Daemon(IRepositoryCleaner repositoryCleaner, IIndexReader indexService)
        {
            _indexService = indexService;
            _repositoryCleaner = repositoryCleaner;
        }

        public void Start(int tickInterval)
        {
            _tickInterval = tickInterval;
            Task.Run(async () => this.Tick());
        }

        private async Task Tick()
        { 
            try 
            {
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
