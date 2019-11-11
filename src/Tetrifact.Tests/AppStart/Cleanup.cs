using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Cleanup : TestBase
    {
        /// <summary>
        /// Ensure that the content of temp folder is wiped when app starts
        /// </summary>
        [Fact]
        public void Basic()
        {
            string tempFilePath = Path.Combine(this.Settings.TempPath, "file");
            File.WriteAllText(tempFilePath, "some text");
            Assert.True(File.Exists(tempFilePath));

            AppLogic appLogic = new AppLogic(this.Settings);
            appLogic.Start();

            Assert.False(File.Exists(tempFilePath));
        }
    }
}

