using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Locks
{
    public class ClearExpired
    {
        private readonly TestContext _testContext = new TestContext();
        
        [Fact]
        public void Happy_path()
        { 
            IProcessManager lockInstance = _testContext.Get<IProcessManager>();
            lockInstance.AddUnique("1", new TimeSpan(0,0,0)); // expires
            lockInstance.AddUnique("2", new TimeSpan(1, 1, 1)); // doesn't expire
            lockInstance.AddUnique("3");
            lockInstance.ClearExpired();
        }
    }
}
