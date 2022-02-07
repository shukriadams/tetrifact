using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Base class for any type which requires concrete file system structures in place
    /// </summary>
    public abstract class FileSystemBase
    {
        protected ISettings Settings;
        protected TestLogger<IIndexReader> Logger;
        protected TestLogger<IWorkspace> WorkspaceLogger;
        protected IIndexReader IndexReader;
        protected ITagsService TagService;
        protected IFileSystem FileSystem;
        protected ThreadDefault ThreadDefault;

        public FileSystemBase()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().Name);

            // this is the "teardown" in our tests - force delete target directory that will contain all disk state for a test run. Requires of course that tests inherit from this class.
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            // pass in real file system for all tests, we use this most of the time, individual tests must override this on their own
            FileSystem = new FileSystem();

            Settings = new Settings(new TestLogger<Settings>())
            {
                RepositoryPath = Path.Join(testFolder, "repository"),
                PackagePath = Path.Join(testFolder, "packages"),
                TempPath = Path.Join(testFolder, "temp"),
                ArchivePath = Path.Join(testFolder, "archives"),
                TagsPath = Path.Join(testFolder, "tags")
            };

            Logger = new TestLogger<IIndexReader>();
            TagService = new Core.TagsService(
                Settings,
                new TestLogger<ITagsService>(), new PackageListCache(MemoryCacheHelper.GetInstance()));

            ThreadDefault = new Core.ThreadDefault();

            IndexReader = new Core.IndexReader(Settings, ThreadDefault, TagService, Logger, FileSystem, HashServiceHelper.Instance());
            Thread.Sleep(200);// yucky fix for race condition when scaffolding up index between consecutive tests
            IndexReader.Initialize();
        }
    }
}
