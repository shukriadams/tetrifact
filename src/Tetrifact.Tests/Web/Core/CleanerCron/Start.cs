using Moq;
using System;
using Tetrifact.Core;
using Xunit;
using W=Tetrifact.Web;

namespace Tetrifact.Tests.Web.Core.Daemon
{
    public class Start
    {
        private TestContext _testContext = new TestContext();

        private readonly MoqHelper _moqHelper;

        public Start()
        {
            _moqHelper = new MoqHelper(_testContext);
        }

        /// <summary>
        /// Coverage test
        /// </summary>
        [Fact]
        public void Happy_path()
        {
            var cleanerCron =_moqHelper.CreateInstance<W.CleanerCron>();
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

            var cleanerCron = _moqHelper.CreateInstanceWithDependency<W.CleanerCron>(archiveService.Object);
            cleanerCron.Work();
        }

    }
}
