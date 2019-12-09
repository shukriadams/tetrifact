using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetPopularTags : Base
    {
        [Fact]
        public void Basic()
        {
            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2003"));
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2002"));
            Directory.CreateDirectory(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2001"));
            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2003", "manifest.json"), JsonConvert.SerializeObject(new Manifest() { Tags = new HashSet<string> { "tag2" } }));
            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2002", "manifest.json"), JsonConvert.SerializeObject(new Manifest() { Tags = new HashSet<string> { "tag2" } }));
            File.WriteAllText(Path.Combine(Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2001", "manifest.json"), JsonConvert.SerializeObject(new Manifest() { Tags = new HashSet<string> { "tag1" } }));


            IEnumerable<string> tags = this.PackageList.GetPopularTags("some-project", 3);
            Assert.Equal("tag2", tags.First());
            Assert.Equal("tag1", tags.ElementAt(1));
        }
    }
}
