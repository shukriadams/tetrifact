using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Base class for any type which requires concrete file system structures in place
    /// </summary>
    public abstract class FileSystemBase : TestBase
    {
        #region FIELDS
        
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

        #endregion

        #region CTORS

        public FileSystemBase()
        {
            // pass in real file system for all tests, we use this most of the time, individual tests must override this on their own
            FileSystem = new FileSystem();
            DirectoryFs = FileSystem.Directory;
            FileFs = FileSystem.File;

            IndexReaderLogger = new TestLogger<IIndexReadService>();
            WorkspaceLogger = new TestLogger<IPackageCreateWorkspace>();
            ArchiveLogger = new TestLogger<IArchiveService>();
            RepoCleanLog = new TestLogger<IRepositoryCleanService>();

            TagService = new Core.TagsService(
                Settings,
                FileSystem,
                new TestLogger<ITagsService>(), new PackageListCache(MemoryCacheHelper.GetInstance()));

            ThreadDefault = new ThreadDefault();

            IndexReader = new IndexReadService(Settings, new TestMemoryCache(), TagService, IndexReaderLogger, FileSystem, HashServiceHelper.Instance(), NinjectHelper.Get<ILock>());
            ArchiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { Settings }); 

            IndexReader.Initialize();
        }

        #endregion
    }
}
