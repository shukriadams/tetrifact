namespace Tetrifact.Web
{
    public interface IDaemon
    {
        void Start(int tickInterval);

        void Dispose();
    }
}
