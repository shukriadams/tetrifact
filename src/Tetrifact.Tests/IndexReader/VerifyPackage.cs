using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class VerifyPackage : FileSystemBase
    {
        [Fact]
        public void Basic() 
        {
            PackageHelper.WriteManifest(Settings, new Core.Manifest { Id= "mypackage" });
           // this.IndexReader.VerifyPackage();
        }
    }
}
