using System.Threading;
using Xunit;

namespace Tetrifact.Tests.WriteLock
{
    public class WaitUntilClear
    {
        /// <summary>
        /// Waituntilclear allows thread to continue
        /// </summary>
        [Fact]
        public void Allow()
        {
            try
            {
                Core.WriteLock.Instance.WaitUntilClear("some-project");
                Assert.True(true);
            }
            finally 
            {
                // MUST release lock after test, else will lock all other tests
                Core.WriteLock.Instance.Clear("some-project");
            }
        }

        /// <summary>
        /// A lock in one thread prevents another thread from continuing
        /// </summary>
        //[Fact] disabled because threading in tests fails in xunit
        public void Wait() 
        {
            Thread worker = null;
            try
            {
                bool continued = false;
                // main thread owns lock now
                Core.WriteLock.Instance.WaitUntilClear("some-project");

                // start a new thread 
                worker = new Thread(() => {
                    Core.WriteLock.Instance.WaitUntilClear("some-project");
                    continued = true;
                });
                worker.Start();

                // give worker time to do stuff
                Thread.Sleep(1000);

                Assert.False(continued);
            }
            finally 
            {
                // MUST release lock after test, else will lock all other tests
                Core.WriteLock.Instance.Clear("some-project");
                worker.Abort();
            }
        }
    }
}
