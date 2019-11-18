using System;
using System.IO;
using System.Text;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Base class for any test type which requires concrete file system structures.
    /// </summary>
    public abstract class FileSystemBase : TestBase
    {
        protected TestLogger<IIndexReader> Logger;
        protected TestLogger<IWorkspace> WorkspaceLogger;
        protected IIndexReader IndexReader;

        /// <summary>
        /// Defines a test package with a single file.
        /// </summary>
        protected class TestPackage
        {
            /// <summary>
            /// Binary content of the single file in test package
            /// </summary>
            public byte[] Content;

            /// <summary>
            /// Path of the single file in test package.
            /// </summary>
            public string Path;

            /// <summary>
            /// Name of the package
            /// </summary>
            public string Name;
        }

        public FileSystemBase()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().FullName);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            Thread.Sleep(200);// race conditio fix


            Settings = new TetriSettings(new TestLogger<TetriSettings>())
            {
                ProjectsPath = Path.Combine(testFolder, Constants.ProjectsFragment),
                TempPath = Path.Combine(testFolder, "temp"),
                TempBinaries = Path.Combine(testFolder, "temp_binaries"),
                ArchivePath = Path.Combine(testFolder, "archives")
            };

            AppLogic appLogic = new AppLogic(Settings);
            appLogic.Start();

            Logger = new TestLogger<IIndexReader>();
            this.IndexReader = new Core.IndexReader(Settings, Logger);
        }

        /// <summary>
        ///  If test requires project to already exist, run this first. Note that creating a package will already do this.
        /// </summary>
        protected void InitProject() 
        {
            Core.Workspace workspace = new Core.Workspace(this.IndexReader, this.Settings, this.WorkspaceLogger);
            workspace.Initialize("some-project"); // init workspace to create project structures
            workspace.Dispose(); // need to dispose to clean up unused workspace folder 
        }

        /// <summary>
        /// Generates a valid package, returns its unique id.
        /// </summary>
        /// <returns></returns>
        protected TestPackage CreatePackage()
        {
            return this.CreatePackage("somepackage");
        }

        protected TestPackage CreatePackage(string packageName)
        {
            // create package, files folder and item location in one
            TestPackage testPackage = new TestPackage
            {
                Content = Encoding.ASCII.GetBytes("some content"),
                Path = $"path\\to\\{packageName}",
                Name = packageName
            };

            this.WorkspaceLogger = new TestLogger<IWorkspace>();
            IWorkspace workspace = new Core.Workspace(this.IndexReader, this.Settings, this.WorkspaceLogger);
            workspace.Initialize("some-project");
            workspace.AddIncomingFile(StreamsHelper.StreamFromBytes(testPackage.Content), testPackage.Path);
            workspace.StageAllFiles(testPackage.Name, null);
            workspace.Finalize("some-project", testPackage.Name, null);

            return testPackage;
        }


    }
}
