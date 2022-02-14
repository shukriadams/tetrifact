using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.LinkLock
{
    public class LinkLock
    {
        public LinkLock()
        {
            Core.LinkLock.Reset();
        }

        [Fact]
        public void DefaultState()
        {
            Assert.False(Core.LinkLock.Instance.IsAnyLocked());
        }

        [Fact]
        public void AnyPackageLocksAll()
        {
            Core.LinkLock.Instance.Lock("some package");
            Assert.True(Core.LinkLock.Instance.IsAnyLocked());
        }

        [Fact]
        public void Unlock()
        {
            Core.LinkLock.Instance.Lock("some package");
            Core.LinkLock.Instance.Unlock("some package");
            Assert.False(Core.LinkLock.Instance.IsAnyLocked());
        }

        [Fact]
        public void OverlappingLocks()
        {
            Core.LinkLock.Instance.Lock("some package");
            Core.LinkLock.Instance.Lock("another package");
            Core.LinkLock.Instance.Unlock("some package");
            Assert.True(Core.LinkLock.Instance.IsAnyLocked());
        }

        [Fact]
        public void OverlappingUnlocks()
        {
            Core.LinkLock.Instance.Lock("some package");
            Core.LinkLock.Instance.Lock("another package");
            Core.LinkLock.Instance.Unlock("another package");
            Core.LinkLock.Instance.Unlock("some package");
            Assert.False(Core.LinkLock.Instance.IsAnyLocked());
        }
    }
}
