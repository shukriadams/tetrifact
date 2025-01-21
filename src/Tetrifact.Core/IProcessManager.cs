using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface IProcessManager
    {
        IEnumerable<ProcessItem> GetCurrent();

        IEnumerable<ProcessItem> GetCurrent(ProcessCategories category);

        bool IsAnyLocked(ProcessCategories category);

        bool IsAnyLocked();

        bool IsLocked(string id);

        void Lock(ProcessCategories category, string id);
        
        void Lock(ProcessCategories category, string id, TimeSpan timespan);

        void ClearExpired();

        void Unlock(string id);

        /// <summary>
        /// Removes all existing processes.
        /// </summary>
        void Clear();
    }
}
