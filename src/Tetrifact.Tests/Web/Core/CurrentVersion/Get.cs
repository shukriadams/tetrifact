using Xunit;

namespace Tetrifact.Tests.Web.Core.CurrentVersion
{
    public class Get
    {
        [Fact]
        public void Happy_path()
        { 
            string version = Tetrifact.Web.CurrentVersion.Get();
        }
    }
}
