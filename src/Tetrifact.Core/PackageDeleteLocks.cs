using System.Collections.Generic;

namespace Tetrifact.Core
{
    public class PackageDeleteLocks
    {
        #region FIELDS

        public static PackageDeleteLocks Instance;

        public IList<string> _lockedPackages { get; }

        #endregion

        #region CTORS

        static PackageDeleteLocks()
        {
            Reset();
        }

        /// <summary>
        /// For testing only
        /// </summary>
        public static void Reset()
        {
            Instance = new PackageDeleteLocks();
        }

        public PackageDeleteLocks()
        {
            _lockedPackages = new List<string>();
        }

        #endregion

        #region METHODS

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
