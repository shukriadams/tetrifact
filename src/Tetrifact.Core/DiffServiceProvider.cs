namespace Tetrifact.Core
{
    public class DiffServiceProvider : IDiffServiceProvider
    {
        private static IDiffService _instance;

        public IDiffService Instance { get { return _instance; } }

        public DiffServiceProvider(IDiffService diffService) 
        {
            if (_instance == null)
                _instance = diffService;
        }
    }
}
