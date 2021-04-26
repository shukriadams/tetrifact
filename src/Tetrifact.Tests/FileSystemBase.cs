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
            return this.CreatePackage("somepackage");
        }

        protected TestPackage CreatePackage(string packageName)
        {
            // create package, files folder and item location in one
            TestPackage testPackage = new TestPackage
            {
                Content = Encoding.ASCII.GetBytes("some content"),
                Path = $"path/to/{packageName}",
                Name = packageName
            };

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

            Settings = new TetriSettings(new TestLogger<TetriSettings>())
            {
                RepositoryPath = Path.Join(testFolder, "repository"),
                PackagePath = Path.Join(testFolder, "packages"),
                TempPath = Path.Join(testFolder, "temp"),
                ArchivePath = Path.Join(testFolder, "archives"),
                TagsPath = Path.Join(testFolder, "tags")
            };

            Logger = new TestLogger<IIndexReader>();
            Core.ITagsService tagService = new Core.TagsService(
                Settings,
                new TestLogger<Core.ITagsService>(), new Core.PackageListCache(MemoryCacheHelper.GetInstance()));

            IndexReader = new Core.IndexReader(Settings, tagService, Logger);
            Thread.Sleep(200);// fixes race condition when scaffolding up index between consecutive tests
            IndexReader.Initialize();
        }
    }
}
