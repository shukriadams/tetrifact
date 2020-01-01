using System.Threading;
using Xunit;

namespace Tetrifact.Tests.WriteLock
{
    public class Clear
    {
        /// <summary>
        /// A lock in one thread can be released allowing another thread to continue
        /// </summary>
        //[Fact] disabled because threading in tests fails in xunit
        public void Continue()
        {
            try
            {
                bool continued = false;

                // main thread owns lock now
                Core.WriteLock.Instance.WaitUntilClear("some-project");

                // start a new thread 
                Thread worker = new Thread(() => {
                    Core.WriteLock.Instance.WaitUntilClear("some-project");
                    continued = true;
                });
                worker.Start();

                // give worker time to do stuff
                Thread.Sleep(100);

                // clear lock
                Core.WriteLock.Instance.Clear("some-project");

                // give worker time to continue
                Thread.Sleep(100);

                Assert.True(continued);
            }
            finally
            {
                Core.WriteLock.Instance.Clear("some-project");
            }

        }
    }
}
