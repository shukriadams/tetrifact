namespace Tetrifact.Web
{
    public interface IDaemonBackgroundProcess
    {
        void Start(DaemonWork work, int tickInterval);

        void Dispose();
    }
}
