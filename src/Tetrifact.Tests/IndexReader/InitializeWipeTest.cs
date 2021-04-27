using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    /// <summary>
    /// Tests temp folder on index initialize (ergo, app start)
    /// </summary>
    public class InitializeTemp
    {
        /// <summary>
        /// Tests that temp folder content is wiped when app starts
        /// </summary>
        [Fact]
        public void Wipe()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().Name);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            Core.ITetriSettings settings = new Core.TetriSettings(new TestLogger<Core.TetriSettings>())
            {
                TempPath = Path.Join(testFolder, "Temp")
            };

            Directory.CreateDirectory(settings.TempPath);
            string testFilePath = Path.Join(settings.TempPath, "test");
            File.WriteAllText(testFilePath, string.Empty);
            Core.ITagsService tagService = new Core.TagsService(
                settings, 
                new TestLogger<Core.ITagsService>(),new Core.PackageListCache(MemoryCacheHelper.GetInstance())); 

            Core.IIndexReader reader = new Core.IndexReader(settings, tagService, new TestLogger<Core.IIndexReader>(), new FileWrapper(new FileSystem()));
            reader.Initialize();

            Assert.False(File.Exists(testFilePath));
        }
    }
}
