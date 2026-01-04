using Newtonsoft.Json;
using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.Workspace
{
    public class FinishManifest
    {
        private readonly TestContext _testContext = new TestContext();

        [Fact]
        public void Basic()
        {
            IPackageCreateWorkspace packageCreateWorkspace = _testContext.Instantiate<IPackageCreateWorkspace>();
            packageCreateWorkspace.Initialize();  

            ISettings settings = _testContext.Instantiate<ISettings>();

            string combinedHash = "somehash";
            string package = "somepackage";
            packageCreateWorkspace.WriteManifest(package, combinedHash);

            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Join(settings.PackagePath, package, "manifest.json")));
            Assert.Equal(manifest.Hash, combinedHash);
        }
    }
}
