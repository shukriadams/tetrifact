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
            IProcessManager lockInstance = TestContext.Get<IProcessManager>();
            lockInstance.Lock(ProcessCategories.Package_Create, "1", new TimeSpan(0,0,0)); // expires
            lockInstance.Lock(ProcessCategories.Package_Create, "2", new TimeSpan(1, 1, 1)); // doesn't expire
            lockInstance.Lock(ProcessCategories.Package_Create, "3");
            lockInstance.ClearExpired();
        }
    }
}
