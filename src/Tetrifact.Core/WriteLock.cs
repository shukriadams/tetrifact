using System.Collections.Generic;
using System.Threading;

namespace Tetrifact.Core
{
    /// <summary>
    /// Used to globally lock writes so only one process can write global state at any given time.
    /// </summary>
    public class WriteLock
    {
        #region FIELDS

        public static WriteLock Instance;

        #endregion

        #region CTORS

        static WriteLock() 
        {
            Instance = new WriteLock();
        }

        #endregion

        #region METHODS

        HashSet<string> _lockList = new HashSet<string>();

        private static readonly object _syncRoot = new object();

        public void WaitUntilClear(string project)
        {
            while (true)
            {
                if (_lockList.Contains(project)) { 
                    Thread.Sleep(100);
                    continue;
                } else {
                    lock (_syncRoot)
                    {
                        _lockList.Add(project);
                    }
                    break;
                }
            }
        }

        public void Clear(string project)
        {
            lock (_syncRoot)
            {
                _lockList.Remove(project);
            }
        }

        #endregion
    }
}
