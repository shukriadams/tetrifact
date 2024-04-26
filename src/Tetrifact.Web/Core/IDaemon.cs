using System;

namespace Tetrifact.Web
{
    public interface IDaemon : IDisposable
    {
        void Start(Cron Daemon);

        void Start(int interval, DaemonWorkMethod work);

        new void Dispose();
    }
}
