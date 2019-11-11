using System.IO;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class Initialize : FileSystemBase
    {
        /// <summary>
        /// Confirms that Initialize() worked - this was called in the CTOR of Base.
        /// </summary>
        [Fact]
        public void InitializeTest()
        {
            Assert.True(Directory.Exists(this.Settings.TempPath));
            Assert.True(Directory.Exists(this.Settings.ArchivePath));
        }
    }
}
