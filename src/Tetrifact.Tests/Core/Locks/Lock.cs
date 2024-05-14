using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Locks
{
    public class Lock : TestBase
    {
        /// <summary>
        /// Ensures that a lock can be reissued without error
        /// </summary>
        [Fact]
        public void Existing()
        { 
            ILock lockInstance = NinjectHelper.Get<ILock>();
            lockInstance.Lock(ProcessLockCategories.Package_Create, "123");
            lockInstance.Lock(ProcessLockCategories.Package_Create, "123");
        }

        /// <summary>
        /// Ensures that a lock can be reissued without error
        /// </summary>
        [Fact]
        public void Existing_with_timeout()
        {
            ILock lockInstance = NinjectHelper.Get<ILock>();
            lockInstance.Lock(ProcessLockCategories.Package_Create, "123", new TimeSpan(1,1,1));
            lockInstance.Lock(ProcessLockCategories.Package_Create, "123", new TimeSpan(1, 1, 1));
        }
    }
}
