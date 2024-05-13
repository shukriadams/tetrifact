using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.LinkLock
{
    public class LinkLock : TestBase
    {
        [Fact]
        public void DefaultState()
        {
            ILock processLock = NinjectHelper.Get<ILock>();
            Assert.False(processLock.IsAnyLocked());
        }

        [Fact]
        public void AnyPackageLocksAll()
        {
            ILock processLock = NinjectHelper.Get<ILock>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            Assert.True(processLock.IsAnyLocked());
        }

        [Fact]
        public void IsNamedLocked()
        {
            ILock processLock = NinjectHelper.Get<ILock>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            Assert.True(processLock.IsLocked("some package"));
        }

        [Fact]
        public void Unlock()
        {
            ILock processLock = NinjectHelper.Get<ILock>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            processLock.Unlock("some package");
            Assert.False(processLock.IsAnyLocked());
        }

        [Fact]
        public void OverlappingLocks()
        {
            ILock processLock = NinjectHelper.Get<ILock>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            processLock.Lock(ProcessLockCategories.Package_Create, "another package");
            processLock.Unlock("some package");
            Assert.True(processLock.IsAnyLocked());
        }

        [Fact]
        public void OverlappingUnlocks()
        {
            ILock processLock = NinjectHelper.Get<ILock>();
            processLock.Lock(ProcessLockCategories.Package_Create, "some package");
            processLock.Lock(ProcessLockCategories.Package_Create, "another package");
            processLock.Unlock("another package");
            processLock.Unlock("some package");
            Assert.False(processLock.IsAnyLocked());
        }
    }
}
