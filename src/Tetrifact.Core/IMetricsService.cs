namespace Tetrifact.Core
{
    public interface IMetricsService
    {
        void Generate();

        string GetInfluxMetrics();
    }
}
