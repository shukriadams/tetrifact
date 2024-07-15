using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    /// <summary>
    /// Used to lock linking globally - this is required for a short period when an incoming package is flipped public.
    /// </summary>
    public class ProcessLockManager : IProcessLockManager
    {
        #region FIELDS

        private readonly Dictionary<string, ProcessLockItem> _items = new Dictionary<string, ProcessLockItem>();

        private readonly ILogger<IProcessLockManager> _log;

        #endregion

        #region CTORS

        public ProcessLockManager(ILogger<IProcessLockManager> log) 
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
        /// gets locks of given category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IEnumerable<ProcessLockItem> GetCurrent(ProcessLockCategories category)
        {
            lock (_items)
                return _items.Where(i => i.Value.Category == category).Select(v => v.Value.Clone());
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
                _log.LogInformation($"Created lock, category {category}, id {id}, no lifespan limit.");
            }
        }

        public void Lock(ProcessLockCategories category, string id, TimeSpan timespan)
        {
            lock (_items)
            {
                if (_items.ContainsKey(id))
                    return;

                _items.Add(id, new ProcessLockItem{ Id = id, AddedUTC = DateTime.UtcNow, MaxLifespan = timespan, Category = category });
                _log.LogInformation($"Created lock, category {category}, id {id}, forced lifespan {timespan}.");
            }
        }

        public void Unlock(string id)
        {
            lock (_items)
            {
                if (!_items.ContainsKey(id))
                    return;

                _items.Remove(id);
                _log.LogInformation($"Cleared lock id {id}.");
            }
        }

        public void Clear() 
        { 
            lock(_items)
            {
                if (_items.Any()) 
                {
                    _log.LogInformation($"Force clearing {_items.Count} lock(s) : {string.Join(",", _items)}.");
                    _items.Clear();
                }
                else 
                { 
                    _log.LogInformation("Force clearing, no locks found.");
                }
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
                _log.LogInformation($"Lock id {key} timed out.");
            }
        }

        #endregion
    }
}
