using Moq;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class PurgeOldArchives
    {
        MoqHelper MoqHelper { get; set; } = new MoqHelper(new TestContext());

        /// <summary>
        /// Covers flow of archive purge when excess archives are present. This is mostly a coverage test
        /// </summary>
        [Fact]
        public void PurgeHappyPath()
        {
            // return two archives paths that are in excess, we need something to iterate over
            Mock<IStorageService> storage = new Mock<IStorageService>();
            storage
                .Setup(r => r.GetExpiredtArchivePaths())
                .Returns(new string[] { "archive-path-1", "archive-path-1" });
            
            // capture delete attempts
            List<string> deleteAttempts = new List<string>();

            Mock<IFileSystem> fileSystem = new Mock<IFileSystem>();
            fileSystem
                .Setup(mq => mq.File.Delete(It.IsAny<string>()))
                .Callback((string path) => {
                    deleteAttempts.Add(path);
                });

            Core.ArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[] { storage, fileSystem });
   
            // do
            archiveService.PurgeOldArchives();

            // test
            Assert.Equal(2, deleteAttempts.Count);
        }
        
        
        /// <summary>
        /// for test coverage - ensures that io exception is thrown
        /// </summary>
        [Fact]
        public void PurgeWithException() 
        {
            // return a archives path in excess, we need something to iterate over
            Mock<IStorageService> storage = new Mock<IStorageService>();
            storage
                .Setup(r => r.GetExpiredtArchivePaths())
                .Returns(new string[] { "archive-path-1" });

            IFileSystem fileSystem = Mock.Of<IFileSystem>();
            Mock
                .Get(fileSystem)
                .Setup(f => f.File.Delete(It.IsAny<string>()))
                .Throws<IOException>();

            Core.ArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[]{ storage });

            // no assert, exception is throttled internally, we just need coverage here
            archiveService.PurgeOldArchives();
        }
        
    }
}
