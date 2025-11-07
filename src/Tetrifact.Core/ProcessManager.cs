using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    /// <summary>
    /// Used to create global in-memory process objects - these can be used to f.egs lock packages, limit simultaneous downloads etc.
    /// </summary>
    public class ProcessManager : IProcessManager
    {
        #region FIELDS

        private readonly Dictionary<string, ProcessItem> _items = new Dictionary<string, ProcessItem>();

        private readonly ILogger<IProcessManager> _log;

        #endregion

        #region PROPERTIES

        public string Context { get; set; } = "unset";

        #endregion

        #region CTORS

        public ProcessManager(ILogger<IProcessManager> log) 
        {
            _log = log;
        }

        #endregion

        #region METHODS

        public int Count()
        {
            lock (_items)
                return _items.Count();
        }

        public bool Any()
        {
            lock (_items)
                return _items.Any();
        }

        public bool AnyOtherThan(string key)
        {
            lock (_items)
                return _items.Any(item => item.Key != key);
        }

        public IEnumerable<ProcessItem> GetAll()
        {
            lock (_items)
                return _items.Values.Select(value => value.Clone());
        }

        public bool AnyOfKeyExists(string key)
        {
            lock (_items)
                return _items.ContainsKey(key);
        }

        public virtual void AddUnique(string key)
        {
            lock (_items)
            {
                if (_items.ContainsKey(key))
                    return;

                _items.Add(key, new ProcessItem { 
                    Id = key, 
                    AddedUTC = DateTime.UtcNow});

                _log.LogInformation($"Created process, id {key}, no lifespan limit, {typeof(ProcessManager).Name}:{this.Context}.");
            }
        }

        public virtual void AddUnique(string key, TimeSpan timespan, string metadata = "")
        {
            lock (_items)
            {
                if (_items.ContainsKey(key))
                    return;

                _items.Add(key, new ProcessItem{ 
                    Id = key,
                    AddedUTC = DateTime.UtcNow,
                    KeepAliveUtc = DateTime.UtcNow,
                    MaxLifespan = timespan,
                    Metadata = metadata
                });

                _log.LogInformation($"Created process, id {key}, metadata {metadata}, forced lifespan {timespan}, {typeof(ProcessManager).Name}:{this.Context}.");
            }
        }

        public void RemoveUnique(string key)
        {
            lock (_items)
            {
                if (!_items.ContainsKey(key))
                    return;

                string meta = _items[key].Metadata;
                _items.Remove(key);
                _log.LogInformation($"Cleared id {key}, meta:{meta}, from {typeof(ProcessManager).Name}:{this.Context}.");
            }
        }

        public void Clear() 
        { 
            lock(_items)
            {
                if (_items.Any()) 
                {
                    _log.LogInformation($"Force clearing {_items.Count} items : {string.Join(",", _items)}, {typeof(ProcessManager).Name}:{this.Context}.");
                    _items.Clear();
                }
                else 
                { 
                    _log.LogInformation($"Force clearing, no processes found,{typeof(ProcessManager).Name}:{this.Context}.");
                }
            }
        }

        public void KeepAlive(string key, string description) 
        {
            lock (_items) 
            {
                if (!_items.ContainsKey(key))
                    return;

                _items[key].KeepAliveUtc = DateTime.UtcNow;
                _items[key].Description = description;
            }
        }

        public void ClearExpired()
        {
            lock (_items) 
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    string key = _items.Keys.ElementAt(_items.Count - 1 - i);

                    // processes need a keepalive value to be eligible for cleanup
                    if (!_items[key].KeepAliveUtc.HasValue)
                       continue;

                    // processes need a maxlifespan to be eligible for cleanup
                    if (!_items[key].MaxLifespan.HasValue)
                        continue;

                    TimeSpan maxLifespan = _items[key].MaxLifespan.Value;
                    DateTime lastUpdate = _items[key].KeepAliveUtc.Value;

                    if (DateTime.UtcNow - lastUpdate < maxLifespan)
                        continue;

                    string meta = _items[key].Metadata;
                    _items.Remove(key);
                    _log.LogInformation($"Process id {key}, meta{meta}, timed out and removed from {typeof(ProcessManager).Name}:{this.Context}.");
                }
            }
        }

        #endregion
    }
}
