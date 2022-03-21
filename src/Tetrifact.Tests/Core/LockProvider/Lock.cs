using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.LockProvider
{
    public class Lock
    {
        /// <summary>
        /// Ensures that a lock can be reissued without error
        /// </summary>
        [Fact]
        public void Existing()
        { 
            ILock lck = new ProcessLock();
            lck.Lock("123");
            lck.Lock("123");
        }

        /// <summary>
        /// Ensures that a lock can be reissued without error
        /// </summary>
        [Fact]
        public void Existing_with_timeout()
        {
            ILock lck = new ProcessLock();
            lck.Lock("123", new TimeSpan(1,1,1));
            lck.Lock("123", new TimeSpan(1, 1, 1));
        }
    }
}
