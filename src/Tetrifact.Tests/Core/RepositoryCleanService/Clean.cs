using System.IO;
using Xunit;
using Tetrifact.Core;
using Moq;
using System;
using System.IO.Abstractions;

namespace Tetrifact.Tests.repositoryCleaner
{
    /// <summary>
    /// Note : renaming this class to just "clean" consistently produces race condition errors
    /// </summary>
    public class Clean : TestBase
    {
        #region CTORS

        public Clean()
        {
            ISettings settings = TestContext.Get<ISettings>();

            // clean tests require all locks released - do this BEFORE constructing repocleaner
            IProcessManager lockInstance = TestContext.Get<IProcessManager>();
            lockInstance.Clear();
        }

        #endregion

        /// <summary>
        /// Arbitrary empty directory should be cleaned out
        /// </summary>
        [Fact]
        public void Clean_Case1()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IRepositoryCleanService repositoryCleaner = TestContext.Get<IRepositoryCleanService>();

            // create arbitrary, empty directory
            string dir = Path.Combine(settings.RepositoryPath, $"an/empty/{Guid.NewGuid()}");
            Directory.CreateDirectory(dir);

            repositoryCleaner.Clean();
    
            Assert.False(Directory.Exists(dir));
        }

        /// <summary>
        /// Test coverage
        /// </summary>
        [Fact]
        public void Clean_Case1_exceptionCover()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            // create artbitrary, empty directory
            string dir = Path.Combine(settings.RepositoryPath, $"an/empty/{Guid.NewGuid()}");
            Directory.CreateDirectory(dir);

            // override concrete dir deletes to throw exception
            Mock<TestDirectory> directoryService = MoqHelper.Mock<TestDirectory>();
            directoryService
                .Setup(r => r.Delete(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IOException>();

            IRepositoryCleanService cleaner = TestContext.Get<IRepositoryCleanService>("indexReader", indexReader, "directoryFileSystem", directoryService.Object, "settings", settings);
            cleaner.Clean();
        }

        /// <summary>
        /// Subscriber file for a package that does exist should NOT be removed
        /// </summary>
        [Fact]
        public void DontClean_case2()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IRepositoryCleanService repositoryCleaner = TestContext.Get<IRepositoryCleanService>();

            // create a package
            TestPackage package = PackageHelper.CreateRandomPackage();

            // case 2 : package subscribed doest not exist
            string dir = Path.Combine(settings.RepositoryPath, $"some/path/{Guid.NewGuid()}.file", "somehash");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "bin"), "I am bin data");
            Directory.CreateDirectory(Path.Combine(dir, "packages"));
            string subscriberFile = Path.Combine(dir, "packages", package.Id);
            File.WriteAllText(subscriberFile, string.Empty); // link package 

            repositoryCleaner.Clean();

            Assert.True(File.Exists(subscriberFile));
        }

        private string Create_Case2_Content()
        {
            ISettings settings = TestContext.Get<ISettings>();

            // case 2 : package subscribed doest not exist
            string dir = Path.Combine(settings.RepositoryPath, $"some/path/{Guid.NewGuid()}.file", "somehash");
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
            IRepositoryCleanService repositoryCleaner = TestContext.Get<IRepositoryCleanService>();

            string subscriberFile = Create_Case2_Content();
            repositoryCleaner.Clean();

            Assert.False(File.Exists(subscriberFile));
        }

        [Fact]
        public void Clean_case2_exceptionCover()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            string subscriberFile = Create_Case2_Content();

            Mock<TestFile> fileservice = MoqHelper.Mock<TestFile>();
            fileservice
                .Setup(r => r.Delete(It.IsAny<string>()))
                .Throws<IOException>();

            IRepositoryCleanService cleaner = TestContext.Get<IRepositoryCleanService>("indexReader", indexReader, "fileFileSystem", fileservice.Object, "settings", settings);
            cleaner.Clean();
        }

        private string Create_case3_content()
        {
            ISettings settings = TestContext.Get<ISettings>();
            string dir = Path.Combine(settings.RepositoryPath, $"some/path/{Guid.NewGuid()}.file", "somehash");
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
            IRepositoryCleanService repositoryCleaner = TestContext.Get<IRepositoryCleanService>();

            string dir = Create_case3_content();
            repositoryCleaner.Clean();
            Assert.False(Directory.Exists(dir));
        }

        [Fact]
        public void Clean_case3_exceptionCoveer()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            string dir = Create_case3_content();

            Mock<TestDirectory> directoryService = MoqHelper.Mock<TestDirectory>();
            directoryService
                .Setup(r => r.Delete(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IOException>();

            IRepositoryCleanService cleaner = TestContext.Get<IRepositoryCleanService>("indexReader", indexReader, "directoryFileSystem", directoryService.Object, "settings", settings);
            cleaner.Clean();
        }

        /// <summary>
        /// Test coverage
        /// </summary>
        [Fact]
        public void Directory_Exception_GetDirectories()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            Mock<TestDirectory> directoryService = MoqHelper.Mock<TestDirectory>();
            directoryService
                .Setup(r => r.GetDirectories(It.IsAny<string>()))
                .Throws<IOException>();

            IRepositoryCleanService mockedCleaner = TestContext.Get<IRepositoryCleanService>("indexReader", indexReader, "directoryFileSystem", directoryService.Object, "settings", settings);
            mockedCleaner.Clean();
        }



        /// <summary>
        /// Clean must exit gracefully with no exception when system locked
        /// </summary>
        [Fact]
        public void Clean_Locked_System()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();
            TestLogger<IRepositoryCleanService> repoCleanLog = new TestLogger<IRepositoryCleanService>();

            // mock out GetAllPackageIds method to force throw exception
            IIndexReadService mockIndexReader = Mock.Of<IIndexReadService>();
            Mock.Get(mockIndexReader)
                .Setup(r => r.GetAllPackageIds())
                .Callback(() => {
                    throw new Exception("System currently locked");
                });

            IRepositoryCleanService respositoryCleaner = TestContext.Get<IRepositoryCleanService>("indexReader", mockIndexReader, "log", repoCleanLog);
            respositoryCleaner.Clean();
            Assert.True(repoCleanLog.ContainsFragment("Clean aborted, lock detected"));
        }

        /// <summary>
        /// Clean must exit gracefully with no exception when system locked
        /// </summary>
        [Fact]
        public void Clean_Unhandled_Exception()
        {
            ISettings settings = TestContext.Get<ISettings>();

            // mock out GetAllPackageIds method to force throw exception
            IIndexReadService mockIndexReader = Mock.Of<IIndexReadService>();
            Mock.Get(mockIndexReader)
                .Setup(r => r.GetAllPackageIds())
                .Callback(() => {
                    throw new Exception("!unhandled!");
                });

            IRepositoryCleanService mockedCleaner = TestContext.Get<IRepositoryCleanService>("indexReader", mockIndexReader);

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
            IProcessManager lockInstance = TestContext.Get<IProcessManager>();
            IRepositoryCleanService repoCleaner = TestContext.Get<IRepositoryCleanService>();

            lockInstance.AddUnique(ProcessCategories.Package_Create, "some-package");
            CleanResult result = repoCleaner.Clean();

            Assert.Contains("Package locks found, clean exited before start", result.Description);
        }
    }
}
