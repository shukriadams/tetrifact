namespace Tetrifact.Core
{
    public interface IPackagePruneService
    {
        /// <summary>
        /// Prunes packages.
        /// </summary>
        void Prune();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        PruneReport Report();
    }
}
