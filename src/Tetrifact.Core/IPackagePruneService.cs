namespace Tetrifact.Core
{
    public interface IPackagePruneService
    {
        /// <summary>
        /// Runs a prune event. 
        /// </summary>
        void Prune();

        /// <summary>
        /// Calculates packages to prune. 
        /// </summary>
        /// <returns></returns>
        PrunePlan GeneratePrunePlan();
    }
}
