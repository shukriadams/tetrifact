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
                return _items.Count > 0;
        }

        public bool IsLocked(string name)
        {
            lock (_items)
                return _items.ContainsKey(name);
        }

        public void Lock(string name)
        {
            if (_items.ContainsKey(name))
                return;

            lock (_items)
                _items.Add(name, null);
        }

        public void Lock(string name, TimeSpan timespan)
        {
            if (_items.ContainsKey(name))
                return;

            lock (_items)
                _items.Add(name, (timespan, DateTime.UtcNow));
        }

        public void Unlock(string name)
        {
            if (!_items.ContainsKey(name))
                return;

            lock (_items)
                _items.Remove(name);
        }

        public void Clear()
        {
            lock (_items)
                _items.Clear();
        }


        public void ClearExpired()
        { 
            for (int i = 0 ; i < _items.Count; i ++)
            {
                string key = _items.Keys.ElementAt(_items.Count - 1 - i);
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
