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
            IProcessLockManager lockInstance = NinjectHelper.Get<IProcessLockManager>();
            lockInstance.Lock(ProcessLockCategories.Package_Create, "123");
            lockInstance.Lock(ProcessLockCategories.Package_Create, "123");
        }

        /// <summary>
        /// Ensures that a lock can be reissued without error
        /// </summary>
        [Fact]
        public void Existing_with_timeout()
        {
            IProcessLockManager lockInstance = NinjectHelper.Get<IProcessLockManager>();
            lockInstance.Lock(ProcessLockCategories.Package_Create, "123", new TimeSpan(1,1,1));
            lockInstance.Lock(ProcessLockCategories.Package_Create, "123", new TimeSpan(1, 1, 1));
        }
    }
}
