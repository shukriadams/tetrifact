using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.AppStart
{
    public class Cleanup : TestBase
    {
        /// <summary>
        /// Content of temp folder must be wiped when app starts
        /// </summary>
        [Fact]
        public void Basic()
        {
            // create a dummy file in temp folder
            string tempFilePath = Path.Combine(this.Settings.TempPath, "file");
            File.WriteAllText(tempFilePath, "some text");

            // start app
            AppLogic appLogic = new AppLogic(this.Settings);
            appLogic.Start();

            // ensure dummy file is gone
            Assert.False(File.Exists(tempFilePath));
        }
    }
}

