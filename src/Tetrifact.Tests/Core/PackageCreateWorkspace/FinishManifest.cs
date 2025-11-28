using Newtonsoft.Json;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class FinishManifest : Base
    {
        private readonly TestContext _testContext = new TestContext();

        [Fact]
        public void Basic()
        {
            ISettings settings = _testContext.Get<ISettings>();

            string combinedHash = "somehash";
            string package = "somepackage";
            this.PackageCreateWorkspace.WriteManifest(package, combinedHash);

            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Join(settings.PackagePath, package, "manifest.json")));
            Assert.Equal(manifest.Hash, combinedHash);
        }
    }
}
