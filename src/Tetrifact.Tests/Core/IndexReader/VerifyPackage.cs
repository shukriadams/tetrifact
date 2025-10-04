using System.IO;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class VerifyPackage : TestBase
    {
        /// <summary>
        /// Happy path - confirms that package verification works
        /// </summary>
        [Fact]
        public void Basic() 
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            PackageHelper.CreateNewPackageFiles("mypackage" );
            (bool, string) result = indexReader.VerifyPackage("mypackage");
            Assert.True(result.Item1);
        }

        /// <summary>
        /// Confirms that attempting to verify a package that does not exist throws a PackageNotFoundException
        /// exception
        /// </summary>
        [Fact]
        public void PackageNotFound()
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            Assert.Throws<PackageNotFoundException>(() =>
            {
                indexReader.VerifyPackage("not-a-valid-package-name");
            });
        }

        /// <summary>
        /// Confirms that package verify catches missing files
        /// </summary>
        [Fact]
        public void FilesMissing()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            // create package
            TestPackage package = PackageHelper.CreateNewPackageFiles("mypackage");

            // delete known package file via disk
            File.Delete(Path.Join(settings.RepositoryPath, package.Path, package.Hash, "bin"));

            (bool, string) result = indexReader.VerifyPackage("mypackage");
            Assert.False(result.Item1);
            Assert.Contains(package.Hash , result.Item2);
            Assert.Contains("could not be found", result.Item2);
        }

        /// <summary>
        /// Verifies that hash mismatch between files on disk vs manifest hash returns error.
        /// </summary>
        [Fact]
        public void FileHashInvalid()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            // create package
            TestPackage package = PackageHelper.CreateNewPackageFiles("mypackage");

            // manually change file on disk after package created
            File.WriteAllText(Path.Join(settings.RepositoryPath, package.Path, package.Hash, "bin"), "some-different-data");

            (bool, string) result = indexReader.VerifyPackage("mypackage");
            Assert.False(result.Item1);
            Assert.Contains("expects hash", result.Item2);
        }

        /// <summary>
        /// Verifies that hash mismatch between package and combined files in package returns error.
        /// </summary>
        [Fact]
        public void PackageHashInvalid()
        {
            IIndexReadService indexReader = TestContext.Get<IIndexReadService>();

            // create package
            TestPackage package = PackageHelper.CreateNewPackageFiles("mypackage");

            // corrupt the final hash in the manifest
            JsonHelper.WriteValuetoRoot(PackageHelper.GetManifestPaths(package.Id), "Hash", "not-a-alid-hash");

            (bool, string) result = indexReader.VerifyPackage("mypackage");
            Assert.False(result.Item1);
            Assert.Contains("does not match expected manifest hash", result.Item2);
        }
    }
}
