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
    public abstract class FileSystemBase : TestBase
    {
        #region FIELDS

        protected ISettings Settings;
        
        protected TestLogger<IIndexReadService> IndexReaderLogger;
        
        protected TestLogger<IPackageCreateWorkspace> WorkspaceLogger;
        
        protected TestLogger<IArchiveService> ArchiveLogger;

        protected TestLogger<IRepositoryCleanService> RepoCleanLog;

        protected IIndexReadService IndexReader;
        
        protected ITagsService TagService;
        
        protected IFileSystem FileSystem;
        
        protected IDirectory DirectoryFs;

        protected IFile FileFs;

        protected ThreadDefault ThreadDefault;
        
        protected IArchiveService ArchiveService;

        protected ILockProvider LockProvider;


        #endregion

        #region CTORS

        public FileSystemBase()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().Name);

            // this is the "teardown" in our tests - force delete target directory that will contain all disk state for a test run. Requires of course that tests inherit from this class.
            int tries = 0;
            while(tries < 100)
            {
                try 
                {
                    if (Directory.Exists(testFolder))
                        Directory.Delete(testFolder, true);

                    break;
                }
                catch
                { 
                    tries++;
                    Thread.Sleep(100);
                }
            }
            if (tries == 100)
                throw new Exception($"failed to delete test folder {testFolder}");

            Directory.CreateDirectory(testFolder);

            // pass in real file system for all tests, we use this most of the time, individual tests must override this on their own
            FileSystem = new FileSystem();
            DirectoryFs = FileSystem.Directory;
            FileFs = FileSystem.File;

            IndexReaderLogger = new TestLogger<IIndexReadService>();
            WorkspaceLogger = new TestLogger<IPackageCreateWorkspace>();
            ArchiveLogger = new TestLogger<IArchiveService>();
            RepoCleanLog = new TestLogger<IRepositoryCleanService>();
            LockProvider = new Core.LockProvider();

            Settings = new Core.Settings()
            {
                RepositoryPath = Path.Join(testFolder, "repository"),
                PackagePath = Path.Join(testFolder, "packages"),
                TempPath = Path.Join(testFolder, "temp"),
                ArchivePath = Path.Join(testFolder, "archives"),
                TagsPath = Path.Join(testFolder, "tags")
            };

            TagService = new Core.TagsService(
                Settings,
                FileSystem,
                new TestLogger<ITagsService>(), new PackageListCache(MemoryCacheHelper.GetInstance()));

            ThreadDefault = new ThreadDefault();

            IndexReader = new IndexReadService(Settings, new TestMemoryCache(), TagService, IndexReaderLogger, FileSystem, HashServiceHelper.Instance(), LockProvider);
            ArchiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { Settings }); // (IndexReader, ThreadDefault, LockProvider, FileSystem, ArchiveLogger, );

            IndexReader.Initialize();
        }

        #endregion
    }
}
