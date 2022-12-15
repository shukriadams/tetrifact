namespace Tetrifact.Web
{
    public interface IDaemonBackgroundProcess
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="work"></param>
        /// <param name="tickInterval">Tick interval in milliseconds</param>
        void Start(DaemonWork work, int tickInterval);

        void Dispose();
    }
}
