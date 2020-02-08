using Xunit;
using Tetrifact.Core;
using System.Collections.Generic;
using Tetrifact.Dev;

namespace Tetrifact.Tests.TagsService
{
    [Collection("Tests")]
    public class RemoveTag : Base 
    {
        [Fact]
        public void Basic(){
            DummyPackage package = this.CreatePackage();
            string tag = "mytag";
            base.TagsService.AddTag("some-project", package.Id, tag);
            base.TagsService.RemoveTag("some-project", package.Id, tag);

            IEnumerable<Package> packages = base.PackageList.GetWithTag("some-project", tag, 0, 10);
            Assert.Empty(packages);
        }
    }
}
