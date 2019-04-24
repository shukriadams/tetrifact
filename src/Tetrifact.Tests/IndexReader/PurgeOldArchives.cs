using System.IO;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class PurgeOldArchives : Base
    {
        [Fact]
        public void PurgeBasic()
        {
            // create fake archives 
            File.WriteAllText(Path.Combine(this.Settings.ArchivePath, "arch1.zip"), string.Empty);
            File.WriteAllText(Path.Combine(this.Settings.ArchivePath, "arch2.zip"), string.Empty);
            File.WriteAllText(Path.Combine(this.Settings.ArchivePath, "arch3.zip"), string.Empty);
            File.WriteAllText(Path.Combine(this.Settings.ArchivePath, "arch4.zip"), string.Empty);
    
            // ensure archives exceed max allowed
            string[] archives = Directory.GetFiles(this.Settings.ArchivePath);
            Assert.True(archives.Length > this.Settings.MaxArchives);

            // purge then ensure one has been deleted, as no more than max
            base.IndexReader.PurgeOldArchives();
            archives = Directory.GetFiles(this.Settings.ArchivePath);
            Assert.Equal(this.Settings.MaxArchives, archives.Length);
        }
    }
}
