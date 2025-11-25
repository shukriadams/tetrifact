using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.LinkLock
{
    public class LinkLock
    {
        private readonly TestContext _testContext = new TestContext();
        
        [Fact]
        public void IsNamedLocked()
        {
            IProcessManager processManager = _testContext.Get<IProcessManager>();
            processManager.AddUnique("some package");
            Assert.True(processManager.HasKey("some package"));
        }
    }
}
