using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.AppStart
{
    [Collection("Tests")]
    public class Start // no base class, clean start required
    {
        #region CTORS

        public Start() 
        {
            string testFolder = TestSetupHelper.SetupDirectories(this);

            Settings.ProjectsPath = Path.Combine(testFolder, Constants.ProjectsFragment);
            Settings.TempPath = Path.Combine(testFolder, "temp");
            Settings.TempBinaries = Path.Combine(testFolder, "temp_binaries");
            Settings.ArchivePath = Path.Combine(testFolder, "archives");

            AppLogic appLogic = new AppLogic();
            appLogic.Start();
        }

        #endregion

        #region TESTS

        /// <summary>
        /// Content of temp folder is wiped when app starts
        /// </summary>
        [Fact]
        public void Basic()
        {
            // create a dummy file in temp folder
            string tempFilePath = Path.Combine(Settings.TempPath, "file");
            File.WriteAllText(tempFilePath, "some text");

            // start app
            AppLogic appLogic = new AppLogic();
            appLogic.Start();

            // ensure dummy file is gone
            Assert.False(File.Exists(tempFilePath));
        }

        #endregion
    }
}

