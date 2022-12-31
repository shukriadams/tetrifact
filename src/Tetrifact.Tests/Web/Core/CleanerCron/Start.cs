using Moq;
using System;
using Tetrifact.Core;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Core.Daemon
{
    public class Start : TestBase
    {
        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            var cleanerCron = MoqHelper.CreateInstanceWithAllMoqed<W.CleanerCron>();
            cleanerCron.Work();
        }

        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Archive_Exception()
        {
            var archiveService = new Mock<IArchiveService>();
            archiveService
                 .Setup(r => r.PurgeOldArchives())
                 .Throws(new Exception("some error"));

            var cleanerCron = MoqHelper.CreateInstanceWithSingleDependency<W.CleanerCron>(archiveService.Object);
            cleanerCron.Work();
        }

    }
}
