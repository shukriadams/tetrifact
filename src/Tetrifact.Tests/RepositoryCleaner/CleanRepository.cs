using System.IO;
using Xunit;
using Tetrifact.Core;
using Moq;
using System;

namespace Tetrifact.Tests.repositoryCleaner
{
    public class CleanRepository : FileSystemBase
    {
        private readonly IRepositoryCleaner _respositoryCleaner;
        private readonly TestLogger<IRepositoryCleaner> _logger;

        private string CreateRepoContent()
        {
            string hash = "somehash";
            string path = "some/path/filename.file";
            string content = "file content";
            string rootPath = Path.Combine(base.Settings.RepositoryPath, path, hash);
            Directory.CreateDirectory(rootPath);
            string filePath = Path.Combine(rootPath, "bin");
            File.WriteAllText(filePath, content);

            return filePath;
        }

        public CleanRepository()
        {
            _logger = new TestLogger<IRepositoryCleaner>();
            _respositoryCleaner = new RepositoryCleaner(this.IndexReader, this.Settings, _logger);
        }

        [Fact]
        public void BasicClean()
        {
            // create a file and write to repository using path convention of path/to/file/bin. File is 
            // not linked to any package
            string contentPath = CreateRepoContent();

            // ensure file exists
            Assert.True(File.Exists(contentPath));

            // assert file is gone after cleaning repo
            _respositoryCleaner.Clean();
            Assert.False(File.Exists(contentPath));
        }

        /// <summary>
        /// Clean must exit gracefully with no exception when system locked
        /// </summary>
        [Fact]
        public void Clean_Locked_System()
        {
            // mock out GetAllPackageIds method to force throw exception
            IIndexReader mockIndexReader = Mock.Of<IIndexReader>();
            Mock.Get(mockIndexReader)
                .Setup(r => r.GetAllPackageIds())
                .Callback(() => {
                    throw new Exception("System currently locked");
                });

            RepositoryCleaner respositoryCleaner = new RepositoryCleaner(mockIndexReader, Settings, _logger);
            respositoryCleaner.Clean();
            Assert.True(_logger.ContainsFragment("Clean aborted, lock detected"));
        }

        /// <summary>
        /// Clean must exit gracefully with no exception when system locked
        /// </summary>
        [Fact]
        public void Clean_Unhandled_Exception()
        {
            // mock out GetAllPackageIds method to force throw exception
            IIndexReader mockIndexReader = Mock.Of<IIndexReader>();
            Mock.Get(mockIndexReader)
                .Setup(r => r.GetAllPackageIds())
                .Callback(() => {
                    throw new Exception("!unhandled!");
                });
            RepositoryCleaner respositoryCleaner = new RepositoryCleaner(mockIndexReader, Settings, _logger);

            Exception ex = Assert.Throws<Exception>(() => {
                respositoryCleaner.Clean();
            });

            Assert.Equal("!unhandled!", ex.Message);
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
