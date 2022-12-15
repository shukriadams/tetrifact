namespace Tetrifact.Web
{
    /// <summary>
    /// A type that implements daemon behaviour.
    /// </summary>
    public interface IDaemon
    {
        /// <summary>
        /// Starts the daemon, has it tick at the given interval.
        /// </summary>
        /// <param name="tickInterval"></param>
        void Start(int tickInterval);

        /// <summary>
        /// Clean up daemon resources.
        /// </summary>
        void Dispose();
    }
}
