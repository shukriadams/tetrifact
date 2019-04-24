using System.Collections.Generic;
using System.IO;
using Xunit;

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
            Directory.CreateDirectory(Path.Combine(this.Settings.PackagePath, "package2"));
            Directory.CreateDirectory(Path.Combine(this.Settings.PackagePath, "package1"));
            Directory.CreateDirectory(Path.Combine(this.Settings.PackagePath, "package3"));

            IEnumerable<string> packages = this.IndexReader.GetPackageIds(0, 1);
            Assert.Single(packages);
            Assert.Contains("package1", packages);
        }

        /// <summary>
        /// Ensures that overrunning the number of existing packages returns an empty page, without error.
        /// </summary>
        [Fact]
        public void SoftOverrun()
        {
            Directory.CreateDirectory(Path.Combine(this.Settings.PackagePath, "package1"));

            // deliberately overshoot number of available packages
            IEnumerable<string> packages = this.IndexReader.GetPackageIds(2, 10); 
            Assert.Empty(packages);
        }
    }
}
