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
            string project = "some-project";
            string combinedHash = "somehash";
            string package = "somepackage";
            this.Workspace.Finalize(project, package, combinedHash);

            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(this.Settings.ProjectsPath, project, Constants.PackagesFragment, package, "manifest.json")));
            Assert.Equal(manifest.Hash, combinedHash);
        }
    }
}
