using System;
using System.IO;
using System.Text;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Base class for any type which requires concrete file system structures in place
    /// </summary>
    public abstract class FileSystemBase
    {
        protected ITetriSettings Settings;
        protected TestLogger<IIndexReader> Logger;
        protected TestLogger<IWorkspace> WorkspaceLogger;
        protected IIndexReader IndexReader;

        protected class TestPackage
        {
            public byte[] Content;
            public string Path;
            public string Name;
        }

        /// <summary>
        /// Generates a valid package, returns its unique id.
        /// </summary>
        /// <returns></returns>
        protected TestPackage CreatePackage()
        {
            TestPackage testPackage = new TestPackage();

            // create package, files folder and item location in one
            testPackage.Content = Encoding.ASCII.GetBytes("some content");
            testPackage.Path = "path/to/file";
            testPackage.Name = "somepackage";

            this.WorkspaceLogger = new TestLogger<IWorkspace>();
            IWorkspace workspace = new Core.Workspace(this.Settings, this.WorkspaceLogger);
            workspace.Initialize();
            workspace.AddIncomingFile(StreamsHelper.StreamFromBytes(testPackage.Content), testPackage.Path);
            workspace.WriteFile(testPackage.Path, "somehash", testPackage.Name);
            workspace.WriteManifest(testPackage.Name, "somehash2");

            return testPackage;
        }

        public FileSystemBase()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().Name);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            Settings = new TetriSettings(new TestLogger<TetriSettings>());
            Settings.RepositoryPath = Path.Join(testFolder, "repository");
            Settings.PackagePath = Path.Join(testFolder, "packages");
            Settings.TempPath = Path.Join(testFolder, "temp");
            Settings.ArchivePath = Path.Join(testFolder, "archives");
            Settings.TagsPath = Path.Join(testFolder, "tags");

            Logger = new TestLogger<IIndexReader>();

            IndexReader = new Core.IndexReader(Settings, Logger);
            Thread.Sleep(200);// fixes race condition when scaffolding up index between consecutive tests
            IndexReader.Initialize();
        }
    }
}
