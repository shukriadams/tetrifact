using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetWithTag : Base
    {
        [Fact]
        public void Basic()
        {
            // tag list work by reading manifest json files on system. Create three manifests,  tag first two with one tag, and last with other tag
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package2003"));
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package2002"));
            Directory.CreateDirectory(Path.Combine(Settings.PackagePath, "package2001"));
            File.WriteAllText(Path.Combine(Settings.PackagePath, "package2003", "manifest.json"), JsonConvert.SerializeObject(new Manifest() { Tags = new HashSet<string> { "tag2" } }));
            File.WriteAllText(Path.Combine(Settings.PackagePath, "package2002", "manifest.json"), JsonConvert.SerializeObject(new Manifest() { Tags = new HashSet<string> { "tag2" } }));
            File.WriteAllText(Path.Combine(Settings.PackagePath, "package2001", "manifest.json"), JsonConvert.SerializeObject(new Manifest() { Tags = new HashSet<string> { "tag1" } }));


            IEnumerable<Package> tags = this.PackageList.GetWithTag("tag2", 0, 2);
            Assert.Equal(2, tags.Count());
            Assert.Contains("tag2", tags.ElementAt(0).Tags);
        }
    }
}
