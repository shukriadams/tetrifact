using System;
using System.Threading;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Locks
{
    public class Lock : TestBase
    {
        [Fact]
        public void Fails_If_Duplicate_Item_Added()
        { 
            IProcessManager lockInstance = TestContext.Get<IProcessManager>();
            lockInstance.AddUnique("123");
            Assert.Throws<Exception>(() =>
            {
                lockInstance.AddUnique("123");
            });
        }

        /// <summary>
        /// Ensures that a lock can be reissued without error
        /// </summary>
        [Fact]
        public void Timeout_Clear_Respects_Item_Timeout()
        {
            IProcessManager lockInstance = TestContext.Get<IProcessManager>();

            // set timeout for 2 seconds
            lockInstance.AddUnique("123", new TimeSpan(0, 0, 0, 2));
            // wait for 1 only
            Thread.Sleep(1000);
            // clear timeout, this should not affect the item just added
            lockInstance.ClearExpired();

            Assert.True(lockInstance.HasKey("123"));
        }

        /// <summary>
        /// Ensures that a lock can be reissued without error
        /// </summary>
        [Fact]
        public void Items_Time_Out()
        {
            IProcessManager lockInstance = TestContext.Get<IProcessManager>();
            lockInstance.AddUnique("123", new TimeSpan(0,0,0,1));
            Thread.Sleep(2000);
            lockInstance.ClearExpired();
            lockInstance.AddUnique("123");
        }
    }
}
