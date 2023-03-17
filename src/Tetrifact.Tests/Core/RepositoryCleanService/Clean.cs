using System.IO;
using Xunit;
using Tetrifact.Core;
using Moq;
using System;

namespace Tetrifact.Tests.repositoryCleaner
{
    /// <summary>
    /// Note : renaming this class to just "clean" consistently produces race condition errors
    /// </summary>
    public class Clean : FileSystemBase
    {
        private readonly IRepositoryCleanService _respositoryCleaner;

        #region CTORS

        public Clean()
        {
            // clean tests require all locks released - do this BEFORE constructing repocleaner
            base.LockProvider.Reset();
            _respositoryCleaner = new RepositoryCleanService(this.IndexReader, LockProvider, this.Settings, this.DirectoryFs, this.FileFs, RepoCleanLog);

        }

        #endregion

        /// <summary>
        /// Arbitrary empty directory should be cleaned out
        /// </summary>
        [Fact]
        public void Clean_Case1()
        {
            // create artbitrary, empty directory
            string dir = Path.Combine(base.Settings.RepositoryPath, $"an/empty/{Guid.NewGuid()}");
            Directory.CreateDirectory(dir);

            _respositoryCleaner.Clean();
    
            Assert.False(Directory.Exists(dir));
        }

        /// <summary>
        /// Test coverage
        /// </summary>
        [Fact]
        public void Clean_Case1_exceptionCover()
        {
            // create artbitrary, empty directory
            string dir = Path.Combine(base.Settings.RepositoryPath, $"an/empty/{Guid.NewGuid()}");
            Directory.CreateDirectory(dir);

            // override concrete dir deletes to throw exception
            Mock<TestDirectory> directoryService = MockRepository.Create<TestDirectory>();
            directoryService
                .Setup(r => r.Delete(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IOException>();

            IRepositoryCleanService cleaner = NinjectHelper.Get<IRepositoryCleanService>(this.Settings, "indexReader", this.IndexReader, "directoryFileSystem", directoryService.Object, "settings", Settings);
            cleaner.Clean();
        }

        /// <summary>
        /// Subscriber file for a package that does exist should NOT be removed
        /// </summary>
        [Fact]
        public void DontClean_case2()
        {
            // create a package
            TestPackage package = PackageHelper.CreateNewPackage(this.Settings);

            // case 2 : package subscribed doest not exist
            string dir = Path.Combine(base.Settings.RepositoryPath, $"some/path/{Guid.NewGuid()}.file", "somehash");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "bin"), "I am bin data");
            Directory.CreateDirectory(Path.Combine(dir, "packages"));
            string subscriberFile = Path.Combine(dir, "packages", package.Id);
            File.WriteAllText(subscriberFile, string.Empty); // link package 

            _respositoryCleaner.Clean();

            Assert.True(File.Exists(subscriberFile));
        }

        private string Create_Case2_Content()
        {
            // case 2 : package subscribed doest not exist
            string dir = Path.Combine(base.Settings.RepositoryPath, $"some/path/{Guid.NewGuid()}.file", "somehash");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "bin"), "I am bin data");
            Directory.CreateDirectory(Path.Combine(dir, "packages"));
            string subscriberFile = Path.Combine(dir, "packages", "deleted-package-id");
            File.WriteAllText(subscriberFile, string.Empty); // link a package that doesn't exist

            return subscriberFile;
        }

        /// <summary>
        /// Subscriber file for a package that doesn't exist should be removed
        /// </summary>
        [Fact]
        public void Clean_case2()
        {
            string subscriberFile = Create_Case2_Content();
            _respositoryCleaner.Clean();

            Assert.False(File.Exists(subscriberFile));
        }

        [Fact]
        public void Clean_case2_exceptionCover()
        {
            string subscriberFile = Create_Case2_Content();

            Mock<TestFile> fileservice = MockRepository.Create<TestFile>();
            fileservice
                .Setup(r => r.Delete(It.IsAny<string>()))
                .Throws<IOException>();

            IRepositoryCleanService cleaner = NinjectHelper.Get<IRepositoryCleanService>(this.Settings, "indexReader", this.IndexReader, "fileFileSystem", fileservice.Object, "settings", Settings);
            cleaner.Clean();
        }

        private string Create_case3_content()
        {
            string dir = Path.Combine(base.Settings.RepositoryPath, $"some/path/{Guid.NewGuid()}.file", "somehash");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "bin"), "I am bin data");
            Directory.CreateDirectory(Path.Combine(dir, "packages"));
            return dir;
        }

        /// <summary>
        /// Bin directory with no subscribers should be deleted
        /// </summary>
        [Fact]
        public void Clean_case3()
        {
            string dir = Create_case3_content();
            _respositoryCleaner.Clean();
            Assert.False(Directory.Exists(dir));
        }

        [Fact]
        public void Clean_case3_exceptionCoveer()
        {
            string dir = Create_case3_content();

            Mock<TestDirectory> directoryService = MockRepository.Create<TestDirectory>();
            directoryService
                .Setup(r => r.Delete(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IOException>();

            IRepositoryCleanService cleaner = NinjectHelper.Get<IRepositoryCleanService>(this.Settings, "indexReader", this.IndexReader, "directoryFileSystem", directoryService.Object, "settings", Settings);
            cleaner.Clean();
        }

        /// <summary>
        /// Test coverage
        /// </summary>
        [Fact]
        public void Directory_Exception_GetDirectories()
        {
            Mock<TestDirectory> directoryService = MockRepository.Create<TestDirectory>();
            directoryService
                .Setup(r => r.GetDirectories(It.IsAny<string>()))
                .Throws<IOException>();

            IRepositoryCleanService mockedCleaner = NinjectHelper.Get<IRepositoryCleanService>(this.Settings, "indexReader", this.IndexReader, "directoryFileSystem", directoryService.Object, "settings", Settings);
            mockedCleaner.Clean();
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

            RepositoryCleanService respositoryCleaner = new RepositoryCleanService(mockIndexReader, this.LockProvider, Settings, this.DirectoryFs, this.FileFs, RepoCleanLog);
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

            RepositoryCleanService mockedCleaner = new RepositoryCleanService(mockIndexReader, LockProvider, Settings, this.DirectoryFs, this.FileFs, RepoCleanLog);

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
            LockProvider.Instance.Lock("some-package");
            _respositoryCleaner.Clean();
            Assert.True(RepoCleanLog.ContainsFragment("Clean aborted, lock detected"));
        }
    }
}
