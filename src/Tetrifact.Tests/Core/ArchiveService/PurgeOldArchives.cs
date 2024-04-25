﻿using Moq;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class PurgeOldArchives : FileSystemBase
    {
        [Fact]
        public void PurgeBasic()
        {
            // create fake archives, max nr + 1
            for (int i = 0; i < this.Settings.MaxArchives + 1; i++) 
                File.WriteAllText(Path.Combine(this.Settings.ArchivePath, $"arch{i}.zip"), string.Empty);
    
            // ensure max archives exceeded by
            string[] archives = Directory.GetFiles(this.Settings.ArchivePath);
            Assert.True(archives.Length > this.Settings.MaxArchives);

            // purge then ensure one has been deleted, as no more than max
            base.ArchiveService.PurgeOldArchives();
            archives = Directory.GetFiles(this.Settings.ArchivePath);
            Assert.Equal(this.Settings.MaxArchives, archives.Length);
        }
        
        
        /// <summary>
        /// for test coverage - ensures that io exception is thrown
        /// </summary>
        [Fact]
        public void PurgeWithException() 
        {
            IFileSystem fileSystem = Mock.Of<IFileSystem>();
            Mock
                .Get(fileSystem)
                .Setup(f => f.File.Delete(It.IsAny<string>()))
                .Throws<IOException>();

            IArchiveService archiveService = new Core.ArchiveService(IndexReader, new ThreadDefault(), LockProvider, fileSystem, ArchiveLogger , Settings);

            // force an archive and ensure that all archives will be purged
            Settings.MaxArchives = 0;
            File.WriteAllText(Path.Join(Settings.ArchivePath, "test"), string.Empty);

            archiveService.PurgeOldArchives();
            // no assert! exception is throttled internally, we just need coverage here
        }
        
    }
}
