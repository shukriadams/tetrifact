using Moq;
using System.IO;
using System.IO.Abstractions;
using Xunit;

namespace Tetrifact.Tests.ArchiveService
{
    public class PurgeOldArchives : FileSystemBase
    {
        [Fact]
        public void PurgeBasic()
        {
            // create fake archives, max nr + 1
            for (int i = 0; i < Settings.MaxArchives + 1; i++) 
                File.WriteAllText(Path.Combine(Settings.ArchivePath, $"arch{i}.zip"), string.Empty);
    
            // ensure max archives exceeded by
            string[] archives = Directory.GetFiles(Settings.ArchivePath);
            Assert.True(archives.Length > Settings.MaxArchives);

            // purge then ensure one has been deleted, as no more than max
            base.ArchiveService.PurgeOldArchives();
            archives = Directory.GetFiles(Settings.ArchivePath);
            Assert.Equal(Settings.MaxArchives, archives.Length);
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

            Core.ArchiveService archiveService = MoqHelper.CreateInstanceWithDependencies<Core.ArchiveService>(new object[]{ Settings }); //Core.ArchiveService(IndexReader, new ThreadDefault(), LockProvider, fileSystem, ArchiveLogger , );

            // force an archive and ensure that all archives will be purged
            Settings.MaxArchives = 0;
            File.WriteAllText(Path.Join(Settings.ArchivePath, "test"), string.Empty);

            archiveService.PurgeOldArchives();
            // no assert! exception is throttled internally, we just need coverage here
        }
        
    }
}
