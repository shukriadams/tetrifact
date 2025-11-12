using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface IProcessManager
    {
        string Context { get; set; }

        IEnumerable<ProcessItem> GetAll();

        bool HasKey(string key);

        bool AnyOtherThanKey(string key);

        bool Any();

        ProcessItem AddUnique(string key);

        ProcessItem AddUnique(string key, TimeSpan timespan, string metadata = "");

        /// <summary>
        /// Gets item with given key. Returns null if doesn't exist.
        /// </summary>
        /// <param name="kkey"></param>
        /// <returns></returns>
        ProcessItem TryFind(string key);

        int Count();

        void ClearExpired();

        void RemoveUnique(string key);

        /// <summary>
        /// Resets to now the creation time of a given process item.
        /// </summary>
        /// <param name="key"></param>
        void KeepAlive(string key, string description);

        /// <summary>
        /// Removes all existing processes. Used by tests.
        /// </summary>
        void Clear();
    }
}
