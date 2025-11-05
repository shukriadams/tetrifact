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

        /// <summary>
        /// Adds an item in a queue if a place is available. Once in queue the item keeps its place until timeout
        /// or removed. Readding the item will succeed if already in queue, but will not update the item's place
        /// in queue.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="timespan"></param>
        /// <param name="key"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        ProcessCreateResponse AddByCategory(ProcessCategories category, TimeSpan timespan, string key, string metadata);

        void AddUnique(ProcessCategories category, string key, TimeSpan timespan);

        // void AddUnique(ProcessCategories category, string key, string metadata, TimeSpan timespan);

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
