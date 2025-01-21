using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface IProcessManager
    {
        IEnumerable<ProcessItem> GetAll();

        IEnumerable<ProcessItem> GetByCategory(ProcessCategories category);

        bool AnyWithCategoryExists(ProcessCategories category);

        bool AnyOfKeyExists(string id);

        void AddUnique(ProcessCategories category, string id);
        
        void AddUnique(ProcessCategories category, string id, TimeSpan timespan);

        void AddUnique(ProcessCategories category, string key, string metadata, TimeSpan timespan);

        void ClearExpired();

        void RemoveUnique(string id);

        /// <summary>
        /// Removes all existing processes.
        /// </summary>
        void Clear();
    }
}
