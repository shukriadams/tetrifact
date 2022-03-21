namespace Tetrifact.Web
{
    public interface IDaemonProcessRunner
    {
        void Start(DaemonWork work, int tickInterval);

        void Dispose();
    }
}
