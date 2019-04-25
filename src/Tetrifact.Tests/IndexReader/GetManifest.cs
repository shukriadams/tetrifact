using Newtonsoft.Json;
using System.IO;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class GetManifest : FileSystemBase
    {
        [Fact]
        public void Get()
        {
            // create package
            string packagePath = Path.Join(Settings.PackagePath, "somepackage");
            Directory.CreateDirectory(packagePath);

            // create manifest
            Core.Manifest manifest = new Core.Manifest
            {
                Hash = "somehash"
            };
            manifest.Files.Add(new Core.ManifestItem { Hash = "itemhash", Path = "path/to/item" });
            File.WriteAllText(Path.Join(packagePath, "manifest.json"), JsonConvert.SerializeObject(manifest));

            Core.Manifest testManifest = this.IndexReader.GetManifest("somepackage");
            Assert.Equal("somehash", testManifest.Hash);
            Assert.Single(testManifest.Files);
            Assert.Equal("itemhash", testManifest.Files[0].Hash);
            Assert.Equal("path/to/item", testManifest.Files[0].Path);
        }

        [Fact]
        public void GetEmpty()
        {
            Core.Manifest testManifest = this.IndexReader.GetManifest("someinvalidpackage");
            Assert.Null(testManifest);
        }
    }
}
