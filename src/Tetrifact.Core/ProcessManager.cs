using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    /// <summary>
    /// Used to lock linking globally - this is required for a short period when an incoming package is flipped public.
    /// </summary>
    public class ProcessManager : IProcessManager
    {
        #region FIELDS

        private readonly Dictionary<string, ProcessItem> _items = new Dictionary<string, ProcessItem>();

        private readonly ILogger<IProcessManager> _log;

        #endregion

        #region CTORS

        public ProcessManager(ILogger<IProcessManager> log) 
        {
            _log = log;
        }

        #endregion

        #region METHODS

        public IEnumerable<ProcessItem> GetAll()
        {
            lock (_items)
                return _items.Values.Select(v => v.Clone());
        }

        /// <summary>
        /// gets locks of given category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IEnumerable<ProcessItem> GetByCategory(ProcessCategories category)
        {
            lock (_items)
                return _items.Where(i => i.Value.Category == category).Select(v => v.Value.Clone());
        }

        /// <summary>
        /// Returns true if any package is locked.
        /// </summary>
        /// <returns></returns>
        public bool AnyWithCategoryExists(ProcessCategories category)
        {
            lock(_items)
                return _items.Where(i => i.Value.Category == category).Any();
        }

        public bool AnyOfKeyExists(string key)
        {
            lock (_items)
                return _items.ContainsKey(key);
        }

        public void AddUnique(ProcessCategories category, string key)
        {
            lock (_items)
            {
                if (_items.ContainsKey(key))
                    return;

                _items.Add(key, new ProcessItem { Id = key, Category = category });
                _log.LogInformation($"Created lock, category {category}, id {key}, no lifespan limit.");
            }
        }

        public void AddUnique(ProcessCategories category, string key, string metadata, TimeSpan timespan)
        {
            lock (_items)
            {
                if (_items.ContainsKey(key))
                    return;

                _items.Add(key, new ProcessItem { Id = key, Metadata = metadata, AddedUTC = DateTime.UtcNow, MaxLifespan = timespan, Category = category });
                _log.LogInformation($"Created lock, category {category}, id {key}, forced lifespan {timespan}.");
            }
        }

        public void AddUnique(ProcessCategories category, string key, TimeSpan timespan)
        {
            lock (_items)
            {
                if (_items.ContainsKey(key))
                    return;

                _items.Add(key, new ProcessItem{ Id = key, AddedUTC = DateTime.UtcNow, MaxLifespan = timespan, Category = category });
                _log.LogInformation($"Created lock, category {category}, id {key}, forced lifespan {timespan}.");
            }
        }

        public void RemoveUnique(string key)
        {
            lock (_items)
            {
                if (!_items.ContainsKey(key))
                    return;

                _items.Remove(key);
                _log.LogInformation($"Cleared lock id {key}.");
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
