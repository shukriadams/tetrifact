using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PackageDeleteLocks
    {
        #region FIELDS

        public static PackageDeleteLocks Instance;

        private readonly IList<string> _lockedPackages;

        #endregion

        #region CTORS

        static PackageDeleteLocks()
        {
            Reset();
        }

        public PackageDeleteLocks()
        {
            _lockedPackages = new List<string>();
        }

        #endregion

        #region METHODS
        
        /// <summary>
        /// For testing only
        /// </summary>
        public static void Reset()
        {
            Instance = new PackageDeleteLocks();
        }

        public void Lock(string packageId)
        {
            lock (_lockedPackages)
            {
                if (!_lockedPackages.Contains(packageId))
                    _lockedPackages.Add(packageId);
            }
        }

        public void Unlock(string packageId)
        {
            lock (Instance)
            {
                if (_lockedPackages.Contains(packageId))
                    _lockedPackages.Remove(packageId);
            }
        }

        public bool IsLocked(string packageId)
        {
            return _lockedPackages.Contains(packageId);
        }

        #endregion
    }
}
