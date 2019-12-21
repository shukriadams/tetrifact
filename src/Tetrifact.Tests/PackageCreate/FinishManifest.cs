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
            string package = "somepackage";
            this.Workspace.Commit(project, package, null, null);

            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(this.Settings.ProjectsPath, project, Constants.ManifestsFragment, package, "manifest.json")));
            Assert.Equal(manifest.Hash, HashService.FromString(string.Empty)); // hash should be empty because no files were added to manifest
            Assert.Empty(manifest.Files);
        }
    }
}
