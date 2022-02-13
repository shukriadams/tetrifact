using Newtonsoft.Json;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class FinishManifest : Base
    {
        [Fact]
        public void Basic()
        {
            string combinedHash = "somehash";
            string package = "somepackage";
            this.PackageCreateWorkspace.WriteManifest(package, combinedHash);

            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Join(this.Settings.PackagePath, package, "manifest.json")));
            Assert.Equal(manifest.Hash, combinedHash);
        }
    }
}
