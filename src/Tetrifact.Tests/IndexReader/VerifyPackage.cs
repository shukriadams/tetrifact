using Xunit;

namespace Tetrifact.Tests.IndexReader
{
    public class VerifyPackage : FileSystemBase
    {
        [Fact]
        public void Basic() 
        {
            PackageHelper.CreatePackage(Settings, "mypackage" );
            (bool, string) result = this.IndexReader.VerifyPackage("mypackage");
            //Assert.True(result.Item1);
        }
    }
}
