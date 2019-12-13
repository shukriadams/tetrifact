using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class Basic
    {
        [Fact]
        public async void CreateLock()
        {
            LockRequest request = new LockRequest();
            await request.Get();
            Assert.True(Core.LinkLock.Instance.IsLocked());
        }

        [Fact]
        public async void Release()
        {
            LockRequest request = new LockRequest();
            await request.Get();
            Core.LinkLock.Instance.Release();
            Assert.False(Core.LinkLock.Instance.IsLocked());
        }

    }
}
