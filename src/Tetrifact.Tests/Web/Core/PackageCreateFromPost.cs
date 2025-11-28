using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Core
{
    public class PackageCreateFromPost
    {
        /// <summary>
        /// coverage
        /// </summary>
        [Fact]
        public void Ctor_happy_path()
        { 
            new W.PackageCreateFromPost();
        }
    }
}
