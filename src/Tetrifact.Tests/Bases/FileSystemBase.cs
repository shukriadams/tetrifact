using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Tetrifact.Dev;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Base for any test type which requires access to the filesystem.
    /// </summary>
    public abstract class FileSystemBase : TestBase
    {
        #region FIELDS

        protected TestLogger<IIndexReader> Logger;
        protected TestLogger<IPackageDeleter> DeleterLogger;
        protected TestLogger<IPackageCreate> PackageCreateLogger;
        protected TestLogger<IProjectService> ProjectServiceLogger;

        protected IIndexReader IndexReader;
        protected IPackageDeleter PackageDeleter;
        protected IPackageCreate PackageCreate;
        protected IProjectService ProjectService;

        #endregion

        #region CTORS

        public FileSystemBase()
        {
            string testFolder = TestSetupHelper.SetupDirectories(this);

            Settings = new Settings()
            {
                ProjectsPath = Path.Combine(testFolder, Constants.ProjectsFragment),
                TempPath = Path.Combine(testFolder, "temp"),
                TempBinaries = Path.Combine(testFolder, "temp_binaries"),
                ArchivePath = Path.Combine(testFolder, "archives")
            };

            AppLogic appLogic = new AppLogic(Settings);
            appLogic.Start();

            this.Logger = new TestLogger<IIndexReader>();
            this.DeleterLogger = new TestLogger<IPackageDeleter>();
            this.ProjectServiceLogger = new TestLogger<IProjectService>();
            this.IndexReader = new Core.IndexReader(Settings, Logger);
            this.PackageDeleter = new Core.PackageDeleter(this.IndexReader, Settings, DeleterLogger, PackageCreateLogger);
            this.PackageCreateLogger = new TestLogger<IPackageCreate>();
            this.PackageCreate = new Core.PackageCreate(this.IndexReader, this.PackageCreateLogger, this.Settings);
            this.ProjectService = new Core.ProjectService(Settings, ProjectServiceLogger);
            this.ProjectService.Create("some-project");
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Generates a valid package, returns its unique id.
        /// </summary>
        /// <returns></returns>
        protected DummyPackage CreatePackage()
        {
            return this.CreatePackage("somepackage");
        }

        /// <summary>
        /// Generates a package with a specific name.
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        protected DummyPackage CreatePackage(string packageName)
        {
            // create package, files folder and item location in one
            DummyPackage testPackage = new DummyPackage(
                packageName, 
                new List<DummyFile>() { 
                    new DummyFile { Content = "some content", Path=  $"path\\to\\{packageName}" } 
                } 
            );
            
            this.PackageCreate.CreateWithValidation(new PackageCreateArguments {
                Files = FormFileHelper.Multiple(testPackage.Files),
                Id = testPackage.Id,
                Project = "some-project"
            });

            return testPackage;
        }

        #endregion

    }
}
