using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.LinkLock
{
    public class LinkLock : TestBase
    {
        [Fact]
        public void DefaultState()
        {
            IProcessManager processManager = TestContext.Get<IProcessManager>();
            Assert.False(processManager.IsAnyLocked());
        }

        [Fact]
        public void AnyPackageLocksAll()
        {
            IProcessManager processManager = TestContext.Get<IProcessManager>();
            processManager.Lock(ProcessCategories.Package_Create, "some package");
            Assert.True(processManager.IsAnyLocked());
        }

        [Fact]
        public void IsNamedLocked()
        {
            IProcessManager processManager = TestContext.Get<IProcessManager>();
            processManager.Lock(ProcessCategories.Package_Create, "some package");
            Assert.True(processManager.IsLocked("some package"));
        }

        [Fact]
        public void Unlock()
        {
            IProcessManager processManager = TestContext.Get<IProcessManager>();
            processManager.Lock(ProcessCategories.Package_Create, "some package");
            processManager.Unlock("some package");
            Assert.False(processManager.IsAnyLocked());
        }

        [Fact]
        public void OverlappingLocks()
        {
            IProcessManager processManager = TestContext.Get<IProcessManager>();
            processManager.Lock(ProcessCategories.Package_Create, "some package");
            processManager.Lock(ProcessCategories.Package_Create, "another package");
            processManager.Unlock("some package");
            Assert.True(processManager.IsAnyLocked());
        }

        [Fact]
        public void OverlappingUnlocks()
        {
            IProcessManager processManager = TestContext.Get<IProcessManager>();
            processManager.Lock(ProcessCategories.Package_Create, "some package");
            processManager.Lock(ProcessCategories.Package_Create, "another package");
            processManager.Unlock("another package");
            processManager.Unlock("some package");
            Assert.False(processManager.IsAnyLocked());
        }
    }
}
