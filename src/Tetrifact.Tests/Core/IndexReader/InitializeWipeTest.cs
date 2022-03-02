using System;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
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
            Core.ISettings settings = new Core.Settings(new TestLogger<Core.Settings>())
            {
                TempPath = Path.Join(testFolder, "Temp")
            };

            IFileSystem filesystem = new FileSystem();

            Directory.CreateDirectory(settings.TempPath);
            string testFilePath = Path.Join(settings.TempPath, "test");
            File.WriteAllText(testFilePath, string.Empty);
            Core.ITagsService tagService = new Core.TagsService(
                settings,
                filesystem,
                new TestLogger<Core.ITagsService>(),new Core.PackageListCache(MemoryCacheHelper.GetInstance())); 

            Core.IIndexReadService reader = new Core.IndexReadService(settings, tagService, new TestLogger<IIndexReadService>(), filesystem, HashServiceHelper.Instance(), new Core.LockProvider());
            reader.Initialize();

            Assert.False(File.Exists(testFilePath));
        }
    }
}
