using System.IO;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class Initialize : Base
    {
        /// <summary>
        /// Tests Initialize() method called in this class' constructor.
        /// </summary>
        [Fact]
        public void InitializeTest()
        {
            Assert.True(Directory.Exists(this.Settings.RepositoryPath));
            Assert.True(Directory.Exists(this.Settings.PackagePath));
            Assert.True(Directory.Exists(this.Settings.TempPath));
        }

    }
}
