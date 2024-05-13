using Xunit;

namespace Tetrifact.Tests.PackageDeleteLocks
{
    public class PackageDeleteLocks : TestBase
    {
        public PackageDeleteLocks()
        {
            Core.PackageDeleteLocks.Reset();
        }

        [Fact]
        public void Lock()
        {
            Core.PackageDeleteLocks.Instance.Lock("somePackage");
            Assert.True(Core.PackageDeleteLocks.Instance.IsLocked("somePackage"));
        }

        [Fact]
        public void Unlock()
        {
            Core.PackageDeleteLocks.Instance.Lock("somePackage");
            Core.PackageDeleteLocks.Instance.Unlock("somePackage");
            Assert.False(Core.PackageDeleteLocks.Instance.IsLocked("somePackage"));
        }
    }
}
