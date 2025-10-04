using Newtonsoft.Json;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetManifest : FileSystemBase
    {
        [Fact]
        public void Get()
        {
            ISettings settings = TestContext.Get<ISettings>();

            // create package
            string packagePath = Path.Join(settings.PackagePath, "somepackage");
            Directory.CreateDirectory(packagePath);

            // create manifest
            Manifest manifest = new Manifest
            {
                Hash = "somehash"
            };
            manifest.Files.Add(new ManifestItem { Hash = "itemhash", Path = "path/to/item" });
            File.WriteAllText(Path.Join(packagePath, "manifest.json"), JsonConvert.SerializeObject(manifest));

            Manifest testManifest = this.IndexReader.GetManifest("somepackage");
            Assert.Equal("somehash", testManifest.Hash);
            Assert.Single(testManifest.Files);
            Assert.Equal("itemhash", testManifest.Files[0].Hash);
            Assert.Equal("path/to/item", testManifest.Files[0].Path);
        }

        /// <summary>
        /// Handles reading a manifest that doesn't exist.
        /// </summary>
        [Fact]
        public void GetEmpty()
        {
            Manifest testManifest = this.IndexReader.GetManifest("someinvalidpackage");
            Assert.Null(testManifest);

            // should not generate a log message
            Assert.Empty(((TestLogger<IIndexReadService>)this.IndexReaderLogger).LogEntries);
        }

        /// <summary>
        /// Handles reading an existing manifest file that isn't a valid json file.
        /// </summary>
        [Fact]
        public void GetInvalidManifet()
        {
            ISettings settings = TestContext.Get<ISettings>();
            string packagefolder = Path.Combine(settings.PackagePath, "someinvalidpackage");
            Directory.CreateDirectory(packagefolder);
            File.WriteAllText(Path.Combine(packagefolder, "manifest.json"), "invalid json!");
            Manifest testManifest = this.IndexReader.GetManifest("someinvalidpackage");
            Assert.Null(testManifest);

            // should generate a error
            Assert.Single(this.IndexReaderLogger.LogEntries);
        }
    }
}
