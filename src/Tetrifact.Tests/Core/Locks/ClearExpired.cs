using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Locks
{
    public class ClearExpired : TestBase
    {
        [Fact]
        public void Happy_path()
        { 
            ILock lockInstance = NinjectHelper.Get<ILock>(SettingsHelper.Get(this.GetType()));
            lockInstance.Lock(ProcessLockCategories.Package_Create, "1", new TimeSpan(0,0,0)); // expires
            lockInstance.Lock(ProcessLockCategories.Package_Create, "2", new TimeSpan(1, 1, 1)); // doesn't expire
            lockInstance.Lock(ProcessLockCategories.Package_Create, "3");
            lockInstance.ClearExpired();
        }
    }
}
