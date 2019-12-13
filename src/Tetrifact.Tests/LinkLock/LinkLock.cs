using Xunit;

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
            Assert.False(Core.LinkLock.Instance.IsLocked());
        }

    }
}
