using System.Collections.Generic;
using System.Linq;

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

        private Dictionary<string, bool> _packageIds = new Dictionary<string, bool>();

        private bool _isLocked;

        private static readonly object _syncRoot = new object();

        public bool IsLocked()
        {
            return _isLocked;
        }

        public void Lock(string packageId)
        {
            lock (_syncRoot)
            {
                _packageIds.Add(packageId, true);
                _isLocked = true;
            }
        }

        public void Unlock(string packageId)
        {
            lock (_syncRoot)
            {
                _packageIds.Remove(packageId);
                _isLocked = _packageIds.Any();
            }
        }

        #endregion
    }
}
