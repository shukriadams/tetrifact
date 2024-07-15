using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.LinkLock
{
    public class LinkLock : TestBase
    {
        [Fact]
        public void DefaultState()
        {
            IProcessLockManager processLock = NinjectHelper.Get<IProcessLockManager>();
            Assert.False(processLock.IsAnyLocked());
        }

        [Fact]
        public void AnyPackageLocksAll()
        {
            IProcessLockManager processLock = NinjectHelper.Get<IProcessLockManager>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            Assert.True(processLock.IsAnyLocked());
        }

        [Fact]
        public void IsNamedLocked()
        {
            IProcessLockManager processLock = NinjectHelper.Get<IProcessLockManager>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            Assert.True(processLock.IsLocked("some package"));
        }

        [Fact]
        public void Unlock()
        {
            IProcessLockManager processLock = NinjectHelper.Get<IProcessLockManager>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            processLock.Unlock("some package");
            Assert.False(processLock.IsAnyLocked());
        }

        [Fact]
        public void OverlappingLocks()
        {
            IProcessLockManager processLock = NinjectHelper.Get<IProcessLockManager>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            processLock.Lock(ProcessLockCategories.Package_Create, "another package");
            processLock.Unlock("some package");
            Assert.True(processLock.IsAnyLocked());
        }

        [Fact]
        public void OverlappingUnlocks()
        {
            IProcessLockManager processLock = NinjectHelper.Get<IProcessLockManager>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            processLock.Lock(ProcessLockCategories.Package_Create, "another package");
            processLock.Unlock("another package");
            processLock.Unlock("some package");
            Assert.False(processLock.IsAnyLocked());
        }
    }
}
