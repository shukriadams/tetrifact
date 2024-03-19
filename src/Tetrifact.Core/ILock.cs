using System;

namespace Tetrifact.Core
{
    public interface ILock
    {
        bool IsAnyLocked(ProcessLockCategories category);

        bool IsAnyLocked();

        bool IsLocked(string id);

        void Lock(ProcessLockCategories category, string id);
        
        void Lock(ProcessLockCategories category, string id, TimeSpan timespan);

        void ClearExpired();

        void Unlock(string id);
    }
}
