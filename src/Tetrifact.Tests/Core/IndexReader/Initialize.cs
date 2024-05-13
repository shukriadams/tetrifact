using System.IO;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class Initialize : FileSystemBase
    {
        /// <summary>
        /// Confirms that Initialize() worked - this was called in the CTOR of Base.
        /// </summary>
        [Fact]
        public void InitializeTest()
        {
            Assert.True(Directory.Exists(SettingsHelper.CurrentSettingsContext.RepositoryPath));
            Assert.True(Directory.Exists(SettingsHelper.CurrentSettingsContext.PackagePath));
            Assert.True(Directory.Exists(SettingsHelper.CurrentSettingsContext.TempPath));
            Assert.True(Directory.Exists(SettingsHelper.CurrentSettingsContext.ArchivePath));
            Assert.True(Directory.Exists(SettingsHelper.CurrentSettingsContext.TagsPath));
        }
    }
}
