using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.LinkLock
{
    public class LinkLock : TestBase
    {
        [Fact]
        public void IsNamedLocked()
        {
            IProcessManager processManager = TestContext.Get<IProcessManager>();
            processManager.AddUnique(ProcessCategories.Package_Create, "some package");
            Assert.True(processManager.AnyOfKeyExists("some package"));
        }
    }
}
