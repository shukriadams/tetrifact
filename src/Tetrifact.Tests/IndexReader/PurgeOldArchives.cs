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

        [Fact]
        public void PurgeLockedArchive()
        {
            Assert.Empty(base.Logger.LogEntries);

            this.Settings.MaxArchives = 0;
            string path = Path.Join(Settings.ArchivePath, "block.zip");
            
            // open stream in write mode to lock it, then attempt to purge archives
            using (FileStream fs = File.OpenWrite(path))
            {
                // force write something to stream to ensure it locks
                fs.Write(Encoding.ASCII.GetBytes("random"));

                base.IndexReader.PurgeOldArchives();
                Assert.Single(base.Logger.LogEntries);
                Assert.Contains("Failed to purge archive", base.Logger.LogEntries[0]);
            }
        }
    }
}
