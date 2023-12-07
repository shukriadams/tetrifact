using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class WriteFile : TestBase
    {
        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void EmptyHash()
        {
            // returns false if attempting to send empty stream 
            IPackageCreateWorkspace workspace = NinjectHelper.Get<IPackageCreateWorkspace>(this.Settings);
            ArgumentException ex = Assert.Throws<ArgumentException>(() => workspace.WriteFile("/fake/file", string.Empty, 0, string.Empty));
            Assert.Equal("Hash value required", ex.Message);
        }
    }
}
