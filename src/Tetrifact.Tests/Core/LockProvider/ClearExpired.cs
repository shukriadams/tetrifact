using System;
using System.Collections.Generic;
using System.Text;
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
            lck.Lock("1", new TimeSpan(0,0,0)); // expires
            lck.Lock("2", new TimeSpan(1, 1, 1)); // doesn't expire
            lck.Lock("3");
            lck.ClearExpired();
        }
    }
}
