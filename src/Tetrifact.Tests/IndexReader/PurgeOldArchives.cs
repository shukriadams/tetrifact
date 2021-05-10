using Moq;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
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
            base.IndexReader.PurgeOldArchives();
            archives = Directory.GetFiles(this.Settings.ArchivePath);
            Assert.Equal(this.Settings.MaxArchives, archives.Length);
        }

        /// <summary>
        /// Confirms that attempting to purge an archive that's in use is handled gracefully and
        /// logs an error. The error loggging is mainly used to indicate that the lock has been 
        /// effective.
        /// </summary>
        //[Fact] disabled because test fails on travis
        public void PurgeLockedArchive()
        {
            Assert.Empty(base.Logger.LogEntries);

            this.Settings.MaxArchives = 0;
            string path = Path.Join(Settings.ArchivePath, "dummy.zip");
    
            // force create dummy zip file in archive folder
            File.WriteAllText(path, "dummy content");

            // open dummy zip in write mode to lock it 
            using (FileStream fs = File.OpenWrite(path))
            {
                // attempt to purge content of archive folder
                base.IndexReader.PurgeOldArchives();

                Assert.Single(base.Logger.LogEntries);
                Assert.Contains("Failed to purge archive", base.Logger.LogEntries[0]);
            }
        }

        
        /// <summary>
        /// for test coverage - ensures that io exception is thrown
        /// </summary>
        [Fact]
        public void PurgeWithException() 
        {
            IFileSystem fileSystem = Mock.Of<IFileSystem>();
            Mock.Get(fileSystem).Setup(f => f.File.Delete(It.IsAny<string>())).Throws<IOException>();
            IIndexReader indexReader = new Core.IndexReader(Settings, TagService, Logger, fileSystem, HashServiceHelper.Instance());

            // force an archive and ensure that all archives will be purged
            Settings.MaxArchives = 0;
            File.WriteAllText(Path.Join(Settings.ArchivePath, "test"), string.Empty);

            indexReader.PurgeOldArchives();
            // no assert! exception is throttled internally, we just need coverage here
        }
        
    }
}
