using System;

namespace Tetrifact.Core
{
    public interface ILock
    {
        bool IsAnyLocked();

        bool IsLocked(string name);

        void Lock(string name);
        
        void Lock(string name, TimeSpan timespan);

        void ClearExpired();

        void Unlock(string name);
    }
}
