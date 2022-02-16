using Moq;
using Tetrifact.Core;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Core.Daemon
{
    public class Start
    {
        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            Mock<W.IDaemonProcessRunner> repoCleanServiceMock = new Mock<W.IDaemonProcessRunner>();
            repoCleanServiceMock
                .Setup(r => r.Start(It.IsAny<W.DaemonWork>(), It.IsAny<int>()))
                .Callback((W.DaemonWork work, int interval)=>{
                    // do work directly, no threading or keep alive, will run once only
                    work();
                });

            Mock<W.IDaemonProcessRunner> processRunner = new Mock<W.IDaemonProcessRunner>();
            Mock<IArchiveService> archiveService = new Mock<IArchiveService>();
            Mock<IRepositoryCleanService> repositoryCleaner = new Mock<IRepositoryCleanService>();
            Mock<IPackagePruneService> packagePrune = new Mock<IPackagePruneService>();

            W.IDaemon daemon = NinjectHelper.Get<W.IDaemon>("repositoryCleaner", repositoryCleaner.Object, "archiveService", archiveService.Object, "processRunner", processRunner.Object, "packagePrune", packagePrune.Object);
            daemon.Start(0);
        }
    }
}
