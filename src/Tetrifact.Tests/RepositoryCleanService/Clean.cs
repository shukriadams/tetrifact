using System.IO;
using Xunit;
using Tetrifact.Core;
using Moq;
using System;
using System.IO.Abstractions;
using System.Threading;
using Ninject;
using Ninject.Parameters;
using System.Collections.Generic;

namespace Tetrifact.Tests.repositoryCleaner
{
    /// <summary>
    /// Note : renaming this class to just "clean" consistently produces race condition errors
    /// </summary>
    public class Clean : FileSystemBase
    {
        private readonly IRepositoryCleanService _respositoryCleaner;

        /// <summary>
        /// Creates 
        /// </summary>
        /// <returns></returns>
        private string CreateRepoContent()
        {
            string hash = "somehash";
            string content = "file content";
            string rootPath = Path.Combine(base.Settings.RepositoryPath, "some/path/filename.file", hash);
            Directory.CreateDirectory(rootPath);
            Directory.CreateDirectory(Path.Combine(base.Settings.RepositoryPath, "an/empty/directory"));
            string filePath = Path.Combine(rootPath, "bin");
            File.WriteAllText(filePath, content);

            return filePath;
        }

        public Clean()
        {
            _respositoryCleaner = new RepositoryCleanService(this.IndexReader, this.Settings, this.DirectoryFs, this.FileFs, RepoCleanLog);
        }

        [Fact]
        public void HappyPath()
        {
            // create a file and write to repository using path convention of path/to/file/bin. File is not linked to any package
            string contentPath = CreateRepoContent();

            // ensure content exists
            Assert.True(File.Exists(contentPath));

            _respositoryCleaner.Clean();

            // content must be gone after cleaning repo
            Thread.Sleep(1500); // wait for slow fs to catch up, todo : rewrite this

            Assert.False(File.Exists(contentPath), contentPath);
        }

        /// <summary>
        /// Clean must exit gracefully with no exception when system locked
        /// </summary>
        [Fact]
        public void Clean_Locked_System()
        {
            // mock out GetAllPackageIds method to force throw exception
            IIndexReadService mockIndexReader = Mock.Of<IIndexReadService>();
            Mock.Get(mockIndexReader)
                .Setup(r => r.GetAllPackageIds())
                .Callback(() => {
                    throw new Exception("System currently locked");
                });

            RepositoryCleanService respositoryCleaner = new RepositoryCleanService(mockIndexReader, Settings, this.DirectoryFs, this.FileFs, RepoCleanLog);
            respositoryCleaner.Clean();
            Assert.True(RepoCleanLog.ContainsFragment("Clean aborted, lock detected"));
        }

        /// <summary>
        /// Clean must exit gracefully with no exception when system locked
        /// </summary>
        [Fact]
        public void Clean_Unhandled_Exception()
        {
            // mock out GetAllPackageIds method to force throw exception
            IIndexReadService mockIndexReader = Mock.Of<IIndexReadService>();
            Mock.Get(mockIndexReader)
                .Setup(r => r.GetAllPackageIds())
                .Callback(() => {
                    throw new Exception("!unhandled!");
                });

            RepositoryCleanService mockedCleaner = new RepositoryCleanService(mockIndexReader, Settings, this.DirectoryFs, this.FileFs, RepoCleanLog);

            Exception ex = Assert.Throws<Exception>(() => {
                mockedCleaner.Clean();
            });

            Assert.Equal("!unhandled!", ex.Message);
        }

        /// <summary>
        /// Coverage test to ensure EnsureNoLock() method is walked
        /// </summary>
        [Fact]
        public void EnsureNoLock_Coverage()
        {
            Core.LinkLock.Instance.Lock("some-package");
            _respositoryCleaner.Clean();
            Assert.True(RepoCleanLog.ContainsFragment("Clean aborted, lock detected"));
        }

        /// <summary>
        /// Test coverage
        /// </summary>
        [Fact]
        public void Directory_Exception_GetDirectories()
        { 
            IFileSystem mockedFilesystem = Mock.Of<IFileSystem>();
            Mock.Get(mockedFilesystem)
                .Setup(r => r.Directory.GetDirectories(It.IsAny<string>()))
                .Throws<IOException>();

            RepositoryCleanService mockedCleaner = new RepositoryCleanService(IndexReader, Settings, mockedFilesystem.Directory, mockedFilesystem.File, RepoCleanLog);
            mockedCleaner.Clean();
            Assert.True(RepoCleanLog.ContainsFragment("Failed to read content of directory"));
        }

        /// <summary>
        /// Test coverage
        /// </summary>
        [Fact]
        public void Directory_Exception_Directory_Delete()
        {
            CreateRepoContent();

            Mock<TestDirectory> dir = MockRepository.Create<TestDirectory>();
            dir
                .Setup(r => r.Delete(It.IsAny<string>()))
                .Throws<IOException>();
            
            IRepositoryCleanService cleaner = NinjectHelper.Get<IRepositoryCleanService>("directoryFileSystem", dir.Object, "settings", Settings, "log", RepoCleanLog);

            cleaner.Clean();
            Assert.True(RepoCleanLog.ContainsFragment("Failed to delete directory"));
        }



        /*
            [Fact]
            public void LinkLocked()
            {
                TestPackage package = base.CreatePackage();
                Core.LinkLock.Instance.Lock(package.Name);
                Settings.LinkLockWaitTime = 1; // millisecond
                int ticks = 0;


                Task.Run(() =>
                {
                    _respositoryCleaner.Clean();
                });

                while(ticks < 10){
                    ticks ++;    
                    Thread.Sleep(10);
                }
                Core.LinkLock.Instance.Lock(package.Name);            
            }
            */
    }
}
