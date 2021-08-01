using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class VerifyPackage : FileSystemBase
    {
        /// <summary>
        /// Happy path - confirms that package verification works
        /// </summary>
        [Fact]
        public void Basic() 
        {
            PackageHelper.CreatePackage(Settings, "mypackage" );
            (bool, string) result = this.IndexReader.VerifyPackage("mypackage");
            Assert.True(result.Item1);
        }

        /// <summary>
        /// Confirms that attempting to verify a package that does not exist throws a PackageNotFoundException
        /// exception
        /// </summary>
        [Fact]
        public void PackageNotFound()
        {
            Assert.Throws<PackageNotFoundException>(() =>
            {
                this.IndexReader.VerifyPackage("not-a-valid-package-name");
            });
        }

        /// <summary>
        /// Confirms that package verify catches missing files
        /// </summary>
        [Fact]
        public void FilesMissing()
        {
            // create package
            TestPackage package = PackageHelper.CreatePackage(Settings, "mypackage");

            // delete known package file via disk
            File.Delete(Path.Join(this.Settings.RepositoryPath, package.Path, package.Hash, "bin"));

            (bool, string) result = this.IndexReader.VerifyPackage("mypackage");
            Assert.False(result.Item1);
            Assert.Contains("Expected package files missing", result.Item2);
        }

        /// <summary>
        /// Verifies that hash mismatch between files on disk vs manifest hash returns error.
        /// </summary>
        [Fact]
        public void HashInvalid()
        {
            // create package
            TestPackage package = PackageHelper.CreatePackage(Settings, "mypackage");

            // manually change file on disk after package created
            File.WriteAllText(Path.Join(this.Settings.RepositoryPath, package.Path, package.Hash, "bin"), "some-different-data");

            (bool, string) result = this.IndexReader.VerifyPackage("mypackage");
            Assert.False(result.Item1);
            Assert.Contains("does not match expected manifest hash", result.Item2);
        }
    }
}
