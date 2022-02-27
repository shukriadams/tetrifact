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
    public class Clean : FileSystemBase
    {
        private readonly IRepositoryCleanService _respositoryCleaner;

        /// <summary>
        /// Creates 
        /// </summary>
        /// <returns></returns>
        private Tuple<string, string, string> CreateRepoContent()
        {
            // create package files
            string rootPathKeep = Path.Combine(base.Settings.RepositoryPath, "some/path/filename.file", "somehash");
            string packageId = "the-package";
            Directory.CreateDirectory(rootPathKeep);
            File.WriteAllText(Path.Combine(rootPathKeep, "bin"), "I am bin data");
            Directory.CreateDirectory(Path.Combine(rootPathKeep, "packages"));
            File.WriteAllText(Path.Combine(rootPathKeep, "packages", packageId), string.Empty); // link a package that doesn't exist

            // create empty folder, this is for coverage testing
            Directory.CreateDirectory(Path.Combine(base.Settings.RepositoryPath, "an/empty/directory"));

            // create bin file with no linked package, this is for coverage testing
            string rootPathDelete = Path.Combine(base.Settings.RepositoryPath, "some/path/abandonedfile.file", "someotherhash");
            Directory.CreateDirectory(rootPathDelete);
            Directory.CreateDirectory(Path.Combine(rootPathDelete, "packages"));
            File.WriteAllText(Path.Combine(rootPathDelete, "bin"), "I am more bin data");

            return new Tuple<string,string, string>(rootPathKeep, rootPathDelete, packageId);
        }

        public Clean()
        {
            _respositoryCleaner = new RepositoryCleanService(this.IndexReader, LockProvider, this.Settings, this.DirectoryFs, this.FileFs, RepoCleanLog);
        }

        /// <summary>
        /// Deletes a package that is not registered as one
        /// </summary>
        [Fact]
        public void HappyPath()
        {
            // create a file and write to repository using path convention of path/to/file/bin. File is not linked to any package
            CreateRepoContent();
            IRepositoryCleanService cleaner = NinjectHelper.Get<IRepositoryCleanService>("settings", Settings);
            cleaner.Clean(); // can't get this to work when run alongside other tests
        }

        /// <summary>
        /// Ensure that package id of placeholder content is marked as valid package, should not be deleted
        /// </summary>
        [Fact]
        public void PackageExists()
        {
            // create a file and write to repository using path convention of path/to/file/bin. File is not linked to any package
            Tuple<string, string, string> content = CreateRepoContent();

            IIndexReadService mockIndexReader = Mock.Of<IIndexReadService>();
            Mock.Get(mockIndexReader)
                .Setup(r => r.GetAllPackageIds())
                .Returns(new []{ content.Item3 });

            // need to delete twice to ensure cascading deletes get a chance to 
            IRepositoryCleanService cleaner = NinjectHelper.Get<IRepositoryCleanService>("indexReader", mockIndexReader, "settings", Settings);
            cleaner.Clean();

            Assert.True(File.Exists(Path.Combine(content.Item1, "packages", content.Item3)));
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

            RepositoryCleanService mockedCleaner = new RepositoryCleanService(IndexReader, LockProvider, Settings, mockedFilesystem.Directory, mockedFilesystem.File, RepoCleanLog);
            mockedCleaner.Clean();
            //Assert.True(RepoCleanLog.ContainsFragment("Failed to read content of directory"));
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
            //Assert.True(RepoCleanLog.ContainsFragment("Failed to delete directory"));
        }
    }
}
