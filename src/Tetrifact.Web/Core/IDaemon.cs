using System;

namespace Tetrifact.Web
{
    public interface IDaemon : IDisposable
    {
        void Start(int interval, DaemonWorkMethod work);

        void Start(string cronmask, DaemonWorkMethod work);

        new void Dispose();
    }
}
