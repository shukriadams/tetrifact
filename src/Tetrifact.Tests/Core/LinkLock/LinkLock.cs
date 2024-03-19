using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.LinkLock
{
    public class LinkLock
    {
        [Fact]
        public void DefaultState()
        {
            ILock processLock = new ProcessLock();
            Assert.False(processLock.IsAnyLocked());
        }

        [Fact]
        public void AnyPackageLocksAll()
        {
            ILock processLock = new ProcessLock();
            processLock.Lock(ProcessLockCategories.Archive_Create, "some package");
            Assert.True(processLock.IsAnyLocked());
        }

        [Fact]
        public void IsNamedLocked()
        {
            ILock processLock = new ProcessLock();
            processLock.Lock(ProcessLockCategories.Archive_Create, "some package");
            Assert.True(processLock.IsLocked("some package"));
        }

        [Fact]
        public void Unlock()
        {
            ILock processLock = new ProcessLock();
            processLock.Lock(ProcessLockCategories.Archive_Create, "some package");
            processLock.Unlock("some package");
            Assert.False(processLock.IsAnyLocked());
        }

        [Fact]
        public void OverlappingLocks()
        {
            ILock processLock = new ProcessLock();
            processLock.Lock(ProcessLockCategories.Archive_Create, "some package");
            processLock.Lock(ProcessLockCategories.Archive_Create, "another package");
            processLock.Unlock("some package");
            Assert.True(processLock.IsAnyLocked());
        }

        [Fact]
        public void OverlappingUnlocks()
        {
            ILock processLock = new ProcessLock();
            processLock.Lock(ProcessLockCategories.Archive_Create, "some package");
            processLock.Lock(ProcessLockCategories.Archive_Create, "another package");
            processLock.Unlock("another package");
            processLock.Unlock("some package");
            Assert.False(processLock.IsAnyLocked());
        }
    }
}
