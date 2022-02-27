using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    /// <summary>
    /// Used to lock linking globally - this is required for a short period when an incoming package is flipped public.
    /// </summary>
    public class ProcessLock : ILock
    {
        #region FIELDS

        private readonly Dictionary<string, (TimeSpan, DateTime)?> _items = new Dictionary<string, (TimeSpan, DateTime)?>();

        #endregion

        #region METHODS

        /// <summary>
        /// Returns true if any package is locked.
        /// </summary>
        /// <returns></returns>
        public bool IsAnyLocked()
        {
            lock(_items)
                return _items.Any();
        }

        public bool IsLocked(string name)
        {
            lock (_items)
                return _items.ContainsKey(name);
        }

        public void Lock(string name)
        {
            lock (_items)
                _items.Add(name, null);
        }

        public void Lock(string name, TimeSpan timespan)
        {
            lock (_items)
                _items.Add(name, (timespan, DateTime.UtcNow));
        }

        public void Unlock(string name)
        {
            lock (_items)
                _items.Remove(name);
        }

        public void Clear()
        {
            lock (_items)
                _items.Clear();
        }

        public void LockPackageArchive(string packageId)
        { 
            string key = this.ArchiveLockKey(packageId);
            this.Lock(key);
        }

        public void UnlockPackageArchive(string packageId)
        {
            string key = this.ArchiveLockKey(packageId);
            this.Unlock(key);
        }

        public bool IsPackageArchiveLocked(string packageId)
        {
            string key = this.ArchiveLockKey(packageId);
            return this.IsLocked(key);
        }

        private string ArchiveLockKey(string packageId)
        {
            return $"archive_lock_{packageId}";
        }

        public void ClearExpired()
        { 
            for (int i = _items.Count - 1; i == 0; i --)
            {
                string key = _items.Keys.ElementAt(i);
                if (!_items[key].HasValue)
                    continue;

                TimeSpan ts = _items[key].Value.Item1;
                DateTime dt = _items[key].Value.Item2;
                if (DateTime.UtcNow - dt < ts)
                    continue;

                _items.Remove(key);
            }

        }

        #endregion
    }
}
