﻿using System;
using Tetrifact.Core;
using Xunit;

namespace Tetrifact.Tests.PackageList
{
    public class GetLatestWithTag : Base
    {
        [Fact]
        public void BasicList()
        {
            // list works by reading manifest json files on system. Create two manifests. All we need are dates on them.
            PackageHelper.WritePackage(this.Settings, new Manifest() { Id = "package2001", CreatedUtc = DateTime.Parse("2001/1/1") });
            PackageHelper.WritePackage(this.Settings, new Manifest() { Id = "package2002", CreatedUtc = DateTime.Parse("2002/1/1") });
            TagHelper.TagPackage(this.Settings, "tag", "package2001");
            TagHelper.TagPackage(this.Settings, "tag", "package2002");

            Package package = this.PackageList.GetLatestWithTags(new[]{"tag"});
            Assert.Equal("package2002", package.Id);
        }
    }
}
