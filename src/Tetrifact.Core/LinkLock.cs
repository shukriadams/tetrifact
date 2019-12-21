using System.Collections.Generic;
using System.Threading;

namespace Tetrifact.Core
{
    /// <summary>
    /// Used to lock linking globally - this is required for a short period when an incoming package is flipped public.
    /// </summary>
    public class LinkLock
    {
        #region FIELDS

        public static LinkLock Instance;

        #endregion

        #region CTORS

        static LinkLock() 
        {
            Reset();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// This is for testing purposes only!
        /// </summary>
        static public void Reset()
        {
            Instance = new LinkLock();
        }

        private readonly Dictionary<string, bool> _packageIds = new Dictionary<string, bool>();

        private bool _isLocked;

        private static readonly object _syncRoot = new object();

        public bool IsLocked()
        {
            return _isLocked;
        }

        public void WaitUntilClear()
        {
            while (true)
            {
                if (_isLocked) { 
                    Thread.Sleep(100);
                    continue;
                } else {
                    lock (_syncRoot)
                    {
                        _isLocked = true;
                    }
                    break;
                }
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _isLocked = false;
            }
        }

        #endregion
    }
}
