﻿using Microsoft.Extensions.Logging;
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

        private readonly Dictionary<string, ProcessLockItem> _items = new Dictionary<string, ProcessLockItem>();

        private readonly ILogger<ILock> _log;

        #endregion

        #region CTORS

        public ProcessLock(ILogger<ILock> log) 
        {
            _log = log;
        }

        #endregion

        #region METHODS

        public IEnumerable<ProcessLockItem> GetCurrent()
        {
            lock (_items)
                return _items.Values.Select(v => v.Clone());
        }

        /// <summary>
        /// Returns true if any package is locked.
        /// </summary>
        /// <returns></returns>
        public bool IsAnyLocked(ProcessLockCategories category)
        {
            lock(_items)
                return _items.Where(i => i.Value.Category == category).Any();
        }

        public bool IsAnyLocked()
        {
            lock (_items)
                return _items.Any();
        }

        public bool IsLocked(string id)
        {
            lock (_items)
                return _items.ContainsKey(id);
        }

        public void Lock(ProcessLockCategories category, string id)
        {
            lock (_items)
            {
                if (_items.ContainsKey(id))
                    return;

                _items.Add(id, new ProcessLockItem { Id = id, Category = category });
                _log.LogInformation($"Created lock, category {category}, id {id}");
            }
        }

        public void Lock(ProcessLockCategories category, string id, TimeSpan timespan)
        {
            lock (_items)
            {
                if (_items.ContainsKey(id))
                    return;

                _items.Add(id, new ProcessLockItem{ Id = id, AddedUTC = DateTime.UtcNow, MaxLifespan = timespan, Category = category });
                _log.LogInformation($"Created lock, category {category}, id {id}, forced lifespan {timespan}");
            }
        }

        public void Unlock(string id)
        {
            lock (_items)
            {
                if (!_items.ContainsKey(id))
                    return;

                _items.Remove(id);
                _log.LogInformation($"Cleared lock id {id}");
            }
        }

        public void Clear() 
        { 
            lock(_items)
            {
                _items.Clear();
                _log.LogInformation($"Force cleared all locks");
            }
        }

        public void ClearExpired()
        {
            for (int i = 0 ; i < _items.Count; i ++)
            {
                string key = _items.Keys.ElementAt(_items.Count - 1 - i);
                if (!_items[key].AddedUTC.HasValue || !_items[key].MaxLifespan.HasValue)
                    continue;

                TimeSpan ts = _items[key].MaxLifespan.Value;
                DateTime dt = _items[key].AddedUTC.Value;
                if (DateTime.UtcNow - dt < ts)
                    continue;

                _items.Remove(key);
                _log.LogInformation($"Lock id {key} timed out");
            }
        }

        #endregion
    }
}
