using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.LockProvider
{
    public class Reset
    {
        // for coveragae
        [Fact]
        public void Happy_path()
        { 
            ILockProvider lockProvider = new Core.LockProvider();
            lockProvider.Reset();
        }
    }
}
