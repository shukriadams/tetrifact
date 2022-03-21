using Xunit;
using W = Tetrifact.Web;

namespace Tetrifact.Tests.Web.Core.Daemon
{
    public class Dispose
    {
        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            W.IDaemon daemon = NinjectHelper.Get<W.IDaemon>();
            daemon.Dispose();
        }
    }
}
