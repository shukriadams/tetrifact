using System.IO;
using Xunit;
using Tetrifact.Core;
using System.Threading;
using System;
using Tetrifact.Dev;

namespace Tetrifact.Tests.Clean
{
    [Collection("Tests")]
    public class Clean : FileSystemBase
    {
        private readonly ICleaner _respositoryCleaner;

        public Clean()
        {
            _respositoryCleaner = new Core.Cleaner(this.IndexReader, new TestLogger<ICleaner>());
        }

        /// <summary>
        /// Transaction folders must be cleaned out
        /// </summary>
        [Fact]
        public void CleanTransactions()
        {
            // create some transaction folders
            string projectPath = PathHelper.GetExpectedProjectPath("some-project");
            string transaction1 = Path.Combine(projectPath, Constants.TransactionsFragment, DateTime.Now.Ticks.ToString());
            Directory.CreateDirectory(transaction1);

            Thread.Sleep(100); // need to wait to give the 2nd transaction time to separate
            string transaction2 = Path.Combine(projectPath, Constants.TransactionsFragment, DateTime.Now.Ticks.ToString());
            Directory.CreateDirectory(transaction2);

            // force transaction preservation depth to preserve 1, then clean
            Settings.TransactionHistoryDepth = 1;
            _respositoryCleaner.Clean("some-project");
            Thread.Sleep(100); // need to wait to give the 2nd transaction time to separate

            // only the latest transaction should exist
            Assert.False(Directory.Exists(transaction1));
            Assert.True(Directory.Exists(transaction2));
        }

        /// <summary>
        /// Removes rehydrated files
        /// </summary>
        [Fact]
        public void CleanRehdyrated() 
        {
            PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package1",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "folder/file")
            });

            PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package2",
                Project = "some-project",
                Files = FormFileHelper.Single("some content2", "folder/file")
            });

            // get file in package2 to force rehdyration, also dispose to ensure stream is closed
            IndexReader.GetFile("some-project", Core.FileIdentifier.Cloak("my package2", "folder/file")).Content.Dispose();

            // confirm rehydrated file exists
            Package package = IndexReader.GetPackage("some-project", "my package2");
            string rehydratedPath = Path.Combine(Settings.TempBinaries, Obfuscator.Cloak("some-project"), package.UniqueId.ToString(), "folder", "file", "bin");
            Assert.True(File.Exists(rehydratedPath));

            // allow deleting of all rehydrated files
            Settings.FilePersistTimeout = 0;
            _respositoryCleaner.Clean("some-project");

            Assert.False(File.Exists(rehydratedPath));
        }

        [Fact]
        public void CleanArchives() 
        {
            PackageCreate.Create(new PackageCreateArguments
            {
                Id = "my package1",
                Project = "some-project",
                Files = FormFileHelper.Single("some content", "folder/file")
            });

            // get package as archive to force build, also dispose to ensure stream is closed
            IndexReader.GetPackageAsArchive("some-project", "my package1").Dispose();

            // confirm archive exists
            Package package = IndexReader.GetPackage("some-project", "my package1");
            string archivePath = Path.Combine(Settings.ArchivePath, Obfuscator.Cloak("some-project"), $"{package.UniqueId}.zip");
            Assert.True(File.Exists(archivePath));

            // allow deleting of all archives 
            Settings.FilePersistTimeout = 0;
            _respositoryCleaner.Clean("some-project");

            Assert.False(File.Exists(archivePath));
        }
    }
}
