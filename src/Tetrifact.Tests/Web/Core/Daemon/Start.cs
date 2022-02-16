using Moq;
using System;
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
            Mock<W.IDaemonProcessRunner> processRunner = new Mock<W.IDaemonProcessRunner>();
            processRunner
                .Setup(r => r.Start(It.IsAny<W.DaemonWork>(), It.IsAny<int>()))
                .Callback((W.DaemonWork work, int interval)=>{
                    // do work directly, no threading or keep alive, will run once only
                    work();
                });

            Mock<IArchiveService> archiveService = new Mock<IArchiveService>();
            Mock<IRepositoryCleanService> repositoryCleaner = new Mock<IRepositoryCleanService>();
            Mock<IPackagePruneService> packagePrune = new Mock<IPackagePruneService>();

            W.IDaemon daemon = NinjectHelper.Get<W.IDaemon>("repositoryCleaner", repositoryCleaner.Object, "archiveService", archiveService.Object, "processRunner", processRunner.Object, "packagePrune", packagePrune.Object);
            daemon.Start(0);
        }

        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Archive_exception()
        {
            Mock<W.IDaemonProcessRunner> processRunner = new Mock<W.IDaemonProcessRunner>();
            processRunner
                .Setup(r => r.Start(It.IsAny<W.DaemonWork>(), It.IsAny<int>()))
                .Callback((W.DaemonWork work, int interval) => {
                    // do work directly, no threading or keep alive, will run once only
                    work();
                });

            Mock<IArchiveService> archiveService = new Mock<IArchiveService>();
            archiveService
                 .Setup(r => r.PurgeOldArchives())
                 .Throws(new Exception("some error"));

            Mock<IRepositoryCleanService> repositoryCleaner = new Mock<IRepositoryCleanService>();
            Mock<IPackagePruneService> packagePrune = new Mock<IPackagePruneService>();

            W.IDaemon daemon = NinjectHelper.Get<W.IDaemon>("repositoryCleaner", repositoryCleaner.Object, "archiveService", archiveService.Object, "processRunner", processRunner.Object, "packagePrune", packagePrune.Object);
            daemon.Start(0);
        }

        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Repo_clean_exception()
        {
            Mock<W.IDaemonProcessRunner> processRunner = new Mock<W.IDaemonProcessRunner>();
            processRunner
                .Setup(r => r.Start(It.IsAny<W.DaemonWork>(), It.IsAny<int>()))
                .Callback((W.DaemonWork work, int interval) => {
                    // do work directly, no threading or keep alive, will run once only
                    work();
                });

            Mock<IRepositoryCleanService> repositoryCleaner = new Mock<IRepositoryCleanService>();
            repositoryCleaner
                 .Setup(r => r.Clean())
                 .Throws(new Exception("some error"));

           
            Mock<IPackagePruneService> packagePrune = new Mock<IPackagePruneService>();
            Mock<IArchiveService> archiveService = new Mock<IArchiveService>();
            W.IDaemon daemon = NinjectHelper.Get<W.IDaemon>("repositoryCleaner", repositoryCleaner.Object, "archiveService", archiveService.Object, "processRunner", processRunner.Object, "packagePrune", packagePrune.Object);
            daemon.Start(0);
        }

        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Prune_exception()
        {
            Mock<W.IDaemonProcessRunner> processRunner = new Mock<W.IDaemonProcessRunner>();
            processRunner
                .Setup(r => r.Start(It.IsAny<W.DaemonWork>(), It.IsAny<int>()))
                .Callback((W.DaemonWork work, int interval) => {
                    // do work directly, no threading or keep alive, will run once only
                    work();
                });


            Mock<IPackagePruneService> packagePrune = new Mock<IPackagePruneService>();
            packagePrune
                 .Setup(r => r.Prune())
                 .Throws(new Exception("some error"));

            Mock<IRepositoryCleanService> repositoryCleaner = new Mock<IRepositoryCleanService>();
            Mock<IArchiveService> archiveService = new Mock<IArchiveService>();
            W.IDaemon daemon = NinjectHelper.Get<W.IDaemon>("repositoryCleaner", repositoryCleaner.Object, "archiveService", archiveService.Object, "processRunner", processRunner.Object, "packagePrune", packagePrune.Object);
            daemon.Start(0);
        }
    }
}
