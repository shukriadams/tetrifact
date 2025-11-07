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
            IProcessManager lockInstance = TestContext.Get<IProcessManager>();
            lockInstance.AddUnique("123");
            lockInstance.AddUnique("123");
        }

        /// <summary>
        /// Ensures that a lock can be reissued without error
        /// </summary>
        [Fact]
        public void Existing_with_timeout()
        {
            IProcessManager lockInstance = TestContext.Get<IProcessManager>();
            lockInstance.AddUnique("123", new TimeSpan(1,1,1));
            lockInstance.AddUnique("123", new TimeSpan(1, 1, 1));
        }
    }
}
