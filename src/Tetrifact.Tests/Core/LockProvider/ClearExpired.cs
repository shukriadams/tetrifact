using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.LockProvider
{
    public class ClearExpired
    {
        [Fact]
        public void Happy_path()
        { 
            ILock lck = new ProcessLock();
            lck.Lock(ProcessLockCategories.Package_Create, "1", new TimeSpan(0,0,0)); // expires
            lck.Lock(ProcessLockCategories.Package_Create, "2", new TimeSpan(1, 1, 1)); // doesn't expire
            lck.Lock(ProcessLockCategories.Package_Create, "3");
            lck.ClearExpired();
        }
    }
}
