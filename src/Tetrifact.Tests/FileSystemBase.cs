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
       
        protected ThreadDefault ThreadDefault;
        
        #endregion

        #region CTORS

        public FileSystemBase()
        {
            IndexReaderLogger = new TestLogger<IIndexReadService>();
            WorkspaceLogger = new TestLogger<IPackageCreateWorkspace>();
            ArchiveLogger = new TestLogger<IArchiveService>();
            RepoCleanLog = new TestLogger<IRepositoryCleanService>();

            ThreadDefault = new ThreadDefault();
        }

        #endregion
    }
}
