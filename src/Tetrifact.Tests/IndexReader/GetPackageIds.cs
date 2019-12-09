using System.Collections.Generic;
using System.IO;
using Xunit;
using Tetrifact.Core;

namespace Tetrifact.Tests.IndexReader
{
    public class GetPackageIds : FileSystemBase
    {
        /// <summary>
        /// Confirms that methods returns a page of directory basenames from within packages directory, sorted by name
        /// </summary>
        [Fact]
        public void GetBasic()
        {
            Directory.CreateDirectory(Path.Combine(this.Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package2"));
            Directory.CreateDirectory(Path.Combine(this.Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package1"));
            Directory.CreateDirectory(Path.Combine(this.Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package3"));

            IEnumerable<string> packages = this.IndexReader.GetPackageIds("some-project", 0, 1);
            Assert.Single(packages);
            Assert.Contains("package1", packages);
        }

        /// <summary>
        /// Ensures that overrunning the number of existing packages returns an empty page, without error.
        /// </summary>
        [Fact]
        public void SoftOverrun()
        {
            Directory.CreateDirectory(Path.Combine(this.Settings.ProjectsPath, "some-project", Constants.ManifestsFragment, "package1"));

            // deliberately overshoot number of available packages
            IEnumerable<string> packages = this.IndexReader.GetPackageIds("some-project", 2, 10); 
            Assert.Empty(packages);
        }
    }
}
