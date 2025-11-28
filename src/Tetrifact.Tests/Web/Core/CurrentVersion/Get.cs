using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Web.Core.CurrentVersion
{
    public class Get
    {
        [Fact]
        public void Happy_path()
        { 
            string version = W.CurrentVersion.Get();
            Assert.Equal("unset", version);
        }
    }
}
