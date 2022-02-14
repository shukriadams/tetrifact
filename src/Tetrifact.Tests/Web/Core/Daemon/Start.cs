using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Core.Daemon
{
    public class Start
    {
        [Fact]
        public void Happy_path()
        { 
            W.Daemon daemon = NinjectHelper.Get<W.Daemon>();
            daemon.Start(0);
        }
    }
}
