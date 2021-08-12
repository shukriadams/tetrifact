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

        private readonly Dictionary<string, bool> _packageIds = new Dictionary<string, bool>();

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

        /// <summary>
        /// Returns true if any package is locked.
        /// </summary>
        /// <returns></returns>
        public bool IsAnyLocked()
        {
            return _packageIds.Any();
        }

        public void Lock(string packageId)
        {
            lock (_packageIds)
            {
                _packageIds.Add(packageId, true);
            }
        }

        public void Unlock(string packageId)
        {
            lock (_packageIds)
            {
                _packageIds.Remove(packageId);
            }
        }

        #endregion
    }
}
