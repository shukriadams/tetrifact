using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.AppStart
{
    [Collection("Tests")]
    public class Initialize : FileSystemBase
    {
        /// <summary>
        /// App creates high-level folder structure on start.
        /// </summary>
        [Fact]
        public void InitializeTest()
        {
            // no need for for setup, FileSystemBase calls AppLogic.Start, all we need to do is confirm folders were created
            Assert.True(Directory.Exists(Settings.ProjectsPath));
            Assert.True(Directory.Exists(Settings.TempPath));
            Assert.True(Directory.Exists(Settings.TempBinaries));
        }
    }
}
