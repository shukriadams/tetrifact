using System.IO;
using System.Text;
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
        [Fact]
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
    }
}
