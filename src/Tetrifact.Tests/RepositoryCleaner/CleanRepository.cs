using System.IO;
using Xunit;
using Tetrifact.Core;
using System.Threading;
using System;

namespace Tetrifact.Tests.RepositoryCleaner
{
    public class CleanRepository : FileSystemBase
    {
        private readonly ICleaner _respositoryCleaner;

        public CleanRepository()
        {
            _respositoryCleaner = new Core.Cleaner(this.IndexReader, this.Settings, new TestLogger<ICleaner>());
        }

        /// <summary>
        /// Transaction folders must be cleaned out
        /// </summary>
        [Fact]
        public void TransactionClean()
        {
            // create some transaction folders
            string projectPath = PathHelper.GetExpectedProjectPath(base.Settings, "some-project");
            string transaction1 = Path.Combine(projectPath, Constants.TransactionsFragment, DateTime.Now.Ticks.ToString());
            Directory.CreateDirectory(transaction1);

            Thread.Sleep(100); // wait to ensure fo
            string transaction2 = Path.Combine(projectPath, Constants.TransactionsFragment, DateTime.Now.Ticks.ToString());
            Directory.CreateDirectory(transaction2);

            // force transaction preservation depth to preserve 1, then clean
            base.Settings.TransactionHistoryDepth = 1;
            _respositoryCleaner.Clean("some-project");
            Thread.Sleep(100); // need to wait to give the 2nd transaction time to separate

            // only the latest transaction should exist
            Assert.False(Directory.Exists(transaction1));
            Assert.True(Directory.Exists(transaction2));
        }
    }
}
