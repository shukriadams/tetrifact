using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface ILock
    {
        IEnumerable<ProcessLockItem> GetCurrent();

        bool IsAnyLocked(ProcessLockCategories category);

        bool IsAnyLocked();

        bool IsLocked(string id);

        void Lock(ProcessLockCategories category, string id);
        
        void Lock(ProcessLockCategories category, string id, TimeSpan timespan);

        void ClearExpired();

        void Unlock(string id);

        /// <summary>
        /// Clears every lock
        /// </summary>
        void Clear();
    }
}
