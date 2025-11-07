using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface IProcessManager
    {
        string Context { get; set; }

        IEnumerable<ProcessItem> GetAll();

        bool AnyOfKeyExists(string key);

        bool AnyOtherThan(string key);

        bool Any();

        void AddUnique(string key);
        
        void AddUnique(string key, TimeSpan timespan, string metadata = "");

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
