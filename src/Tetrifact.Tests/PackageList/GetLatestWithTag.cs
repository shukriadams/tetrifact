using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetLatestWithTag : Base
    {
        /// <summary>
        /// GetLatestWithTag returns the last package with a given tag. 
        /// </summary>
        [Fact]
        public void BasicList()
        {
            this.CreatePackage("package2002");
            this.CreatePackage("package2003");

            // tag a package, GetLatestWithTag should return it
            this.TagService.AddTag("some-project", "package2002", "tag");
            Package package = this.PackageList.GetLatestWithTag("some-project", "tag");
            Assert.Equal("package2002", package.Id);

            // tag another package, GetLatestWithTag should return that instead of the previos package
            this.TagService.AddTag("some-project", "package2003", "tag");
            package = this.PackageList.GetLatestWithTag("some-project", "tag");
            Assert.Equal("package2003", package.Id);
        }
    }
}
