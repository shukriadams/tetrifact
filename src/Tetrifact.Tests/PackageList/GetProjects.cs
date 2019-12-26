using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetProjects // No base - we need a totally
    {
        ISettings Settings;
        IPackageList PackageList;

        #region CTOR

        /// <summary>
        /// Set up clean base file system, don't create any existi
        /// </summary>
        public GetProjects() 
        {
            // we need a folder to work in
            string testFolder = TestSetupHelper.SetupDirectories(this);

            // bind settings to that folder
            Settings = new Settings()
            {
                ProjectsPath = Path.Combine(testFolder, Constants.ProjectsFragment),
                TempPath = Path.Combine(testFolder, "temp"),
                TempBinaries = Path.Combine(testFolder, "temp_binaries"),
                ArchivePath = Path.Combine(testFolder, "archives")
            };
            
            // initialize app, this is always needed
            AppLogic appLogic = new AppLogic(Settings);
            appLogic.Start();

            // we'll be using indexreader for all tests
            IIndexReader indexReader = new Core.IndexReader(Settings, new TestLogger<IIndexReader>() );
            this.PackageList = new Core.PackageList(MemoryCacheHelper.GetInstance(), indexReader, Settings, new TestLogger<IPackageList>());
        }

        #endregion

        /// <summary>
        /// Retrieves a project
        /// </summary>
        [Fact]
        public void Basic() 
        {
            string project = "my-project";
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project)));

            IEnumerable<string> projects = this.PackageList.GetProjects();
            Assert.Single(projects);
            Assert.Contains(project, projects);
        }

        /// <summary>
        /// A project name can contain characters which cannot be written to file system
        /// </summary>
        [Fact]
        public void IllegalCharacterSupport()
        {
            string project = "* ! : \\ //";
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project)));

            IEnumerable<string> projects = this.PackageList.GetProjects();
            Assert.Single(projects);
            Assert.Contains(project, projects);
        }

        /// <summary>
        /// GetProjects returns an empty list if no projects exist.
        /// </summary>
        [Fact]
        public void Empty()
        {
            IEnumerable<string> projects = PackageList.GetProjects();
            Assert.Empty(projects);
        }
    }
}
