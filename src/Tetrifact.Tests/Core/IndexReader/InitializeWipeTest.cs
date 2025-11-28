using System;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    /// <summary>
    /// Tests temp folder on index initialize (ergo, app start)
    /// </summary>
    public class InitializeTemp
    {
        private TestContext _testContext = new TestContext();
        
        private MoqHelper _moqHelper;
        
        public InitializeTemp()
        {
            _moqHelper = new MoqHelper(_testContext);
        }
        
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
            ISettings settings = _testContext.Get<ISettings>();
            settings.TempPath = Path.Join(testFolder, "Temp");

            Directory.CreateDirectory(settings.TempPath);
            string testFilePath = Path.Join(settings.TempPath, "test");
            File.WriteAllText(testFilePath, string.Empty);

            IIndexReadService reader = _moqHelper.CreateInstanceWithDependencies<IndexReadService>(new object[] { settings });
            reader.Initialize();

            Assert.False(File.Exists(testFilePath));
        }
    }
}
