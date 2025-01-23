using System;

namespace Tetrifact.Web
{
    public interface IDaemon : IDisposable
    {
        /// <summary>
        /// Runs a daemon process at interval (millisecond) intervals.
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="work"></param>
        void Start(int interval, DaemonWorkMethod work);

        /// <summary>
        /// Runs a daemon process based on a cron mask string.
        /// </summary>
        /// <param name="cronmask"></param>
        /// <param name="work"></param>
        void Start(string cronmask, DaemonWorkMethod work);

        new void Dispose();
    }
}
