using System;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    /// <summary>
    /// Tests temp folder on index initialize (ergo, app start)
    /// </summary>
    public class InitializeTemp : TestBase
    {
        /// <summary>
        /// Tests that temp folder content is wiped when app starts
        /// </summary>
        [Fact]
        public void Wipe()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "__testdata", Guid.NewGuid().ToString());
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);
            ISettings settings = TestContext.Get<ISettings>();
            settings.TempPath = Path.Join(testFolder, "Temp");

            Directory.CreateDirectory(settings.TempPath);
            string testFilePath = Path.Join(settings.TempPath, "test");
            File.WriteAllText(testFilePath, string.Empty);

            IIndexReadService reader = MoqHelper.CreateInstanceWithDependencies<IndexReadService>(new object[] { settings });
            reader.Initialize();

            Assert.False(File.Exists(testFilePath));
        }
    }
}
