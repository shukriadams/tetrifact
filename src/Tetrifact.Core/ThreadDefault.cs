using System.Threading;

namespace Tetrifact.Core
{
    public class ThreadDefault : IThread
    {
        public void Sleep(int ms)
        { 
            Thread.Sleep(ms);
        }
    }
}
