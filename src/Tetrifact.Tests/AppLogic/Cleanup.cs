using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.AppStart
{
    public class Cleanup 
    {
        #region FIELDS

        private ITetriSettings Settings;

        #endregion

        #region CTORS

        public Cleanup() 
        {
            string testFolder = TestSetupHelper.SetupDirectories(this);

            this.Settings = new TetriSettings()
            {
                ProjectsPath = Path.Combine(testFolder, Constants.ProjectsFragment),
                TempPath = Path.Combine(testFolder, "temp"),
                TempBinaries = Path.Combine(testFolder, "temp_binaries"),
                ArchivePath = Path.Combine(testFolder, "archives")
            };

            AppLogic appLogic = new AppLogic(Settings);
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
            string tempFilePath = Path.Combine(this.Settings.TempPath, "file");
            File.WriteAllText(tempFilePath, "some text");

            // start app
            AppLogic appLogic = new AppLogic(this.Settings);
            appLogic.Start();

            // ensure dummy file is gone
            Assert.False(File.Exists(tempFilePath));
        }

        #endregion
    }
}

