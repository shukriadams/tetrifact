using System;

namespace Tetrifact.Web
{
    public interface IDaemon : IDisposable
    {
        void Start(Cron Daemon);

        new void Dispose();
    }
}
