using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;
using Tetrifact.Dev;
using Ninject;
using System.Reflection;
using System;

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

            Settings.ProjectsPath = Path.Combine(testFolder, Constants.ProjectsFragment);
            Settings.TempPath = Path.Combine(testFolder, "temp");
            Settings.TempBinaries = Path.Combine(testFolder, "temp_binaries");
            Settings.ArchivePath = Path.Combine(testFolder, "archives");

            AppLogic appLogic = new AppLogic();
            appLogic.Start();
            

            StandardKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            this.Logger = new TestLogger<IIndexReader>();
            this.DeleterLogger = new TestLogger<IPackageDeleter>();
            this.ProjectServiceLogger = new TestLogger<IProjectService>();
            this.IndexReader = new Core.IndexReader(Logger);
            this.PackageDeleter = kernel.Get<Core.PackageDeleter>();
            this.PackageCreateLogger = new TestLogger<IPackageCreate>();
            this.PackageCreate = kernel.Get<Core.PackageCreate>();
            this.ProjectService = new Core.ProjectService(new Core.PackageList(MemoryCacheHelper.GetInstance(), this.IndexReader, new MemoryLogger<IPackageList>()));
            this.ProjectService.CreateProject("some-project");
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
                    new DummyFile { Content = "some content", Path=  $"path/to/{packageName}" } 
                } 
            );
            
            PackageCreateResult result = this.PackageCreate.Create(new PackageCreateArguments {
                Files = FormFileHelper.Multiple(testPackage.Files),
                Id = testPackage.Id,
                Project = "some-project"
            });

            if (!result.Success)
                throw new Exception("Package creation failed");

            return testPackage;
        }

        #endregion

    }
}
