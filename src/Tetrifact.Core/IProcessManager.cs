using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface IProcessManager
    {
        IEnumerable<ProcessItem> GetAll();

        IEnumerable<ProcessItem> GetByCategory(ProcessCategories category);

        bool AnyWithCategoryExists(ProcessCategories category);

        bool AnyOfKeyExists(string key);

        void AddUnique(ProcessCategories category, string key);

        ProcessCreateResponse AddRestrained(ProcessCategories category, string key, string metadata);

        void AddUnique(ProcessCategories category, string key, TimeSpan timespan);

        void AddUnique(ProcessCategories category, string key, string metadata, TimeSpan timespan);

        void ClearExpired();

        void RemoveUnique(string key);

        /// <summary>
        /// Resets to now the creation time of a given process item.
        /// </summary>
        /// <param name="key"></param>
        void KeepAlive(string key, string description);

        /// <summary>
        /// Removes all existing processes.
        /// </summary>
        void Clear();
    }
}
