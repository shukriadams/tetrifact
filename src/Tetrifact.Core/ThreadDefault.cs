using System.Threading;

namespace Tetrifact.Core
{
    public class ThreadDefault : IThread
    {
        public void Sleep(int ms)
        { 
            if (ms > 0)
                Thread.Sleep(ms);
        }
    }
}
