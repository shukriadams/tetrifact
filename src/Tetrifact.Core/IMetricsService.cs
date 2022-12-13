namespace Tetrifact.Core
{
    public interface IMetricsService
    {
        /// <summary>
        /// Generates metrics for all supported platforms.
        /// </summary>
        void Generate();

        /// <summary>
        /// Retrieve metrics for Influx. Throws MetricsStaleException if metrics cannot be retrieved.
        /// </summary>
        /// <returns></returns>
        string GetInfluxMetrics();
    }
}
