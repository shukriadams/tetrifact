using System.IO;
using Xunit;

namespace Tetrifact.Tests.AppStart
{
    public class Initialize : FileSystemBase
    {
        /// <summary>
        /// App creates high-level folder structure on start.
        /// </summary>
        [Fact]
        public void InitializeTest()
        {
            // no need for for setup, FileSystemBase calls AppLogic.Start, all we need to do is confirm folders were created
            Assert.True(Directory.Exists(this.Settings.ProjectsPath));
            Assert.True(Directory.Exists(this.Settings.TempPath));
            Assert.True(Directory.Exists(this.Settings.TempBinaries));
        }
    }
}
