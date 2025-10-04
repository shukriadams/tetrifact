using System.IO;
using Tetrifact.Core;
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
            ISettings settings = TestContext.Get<ISettings>();
            Assert.True(Directory.Exists(settings.RepositoryPath));
            Assert.True(Directory.Exists(settings.PackagePath));
            Assert.True(Directory.Exists(settings.TempPath));
            Assert.True(Directory.Exists(settings.ArchivePath));
            Assert.True(Directory.Exists(settings.TagsPath));
        }
    }
}
