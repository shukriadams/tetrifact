using Tetrifact.Core;
using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Web.Core.Pager
{
    public class Render : TestBase
    {
        [Fact]
        public void Happy_path()
        { 
            W.Pager pager = new W.Pager();
            string pagebar = pager.Render<string>(new PageableData<string>( new string[] { "1", "2", "3", "4", "5" }, 4, 2, 5 ), 1, string.Empty, string.Empty);
            Assert.NotEmpty(pagebar);
        }
    }
}
