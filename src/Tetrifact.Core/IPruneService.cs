namespace Tetrifact.Core
{
    public interface IPruneService
    {
        /// <summary>
        /// Runs a prune event. 
        /// </summary>
        PrunePlan Prune();

        /// <summary>
        /// Calculates packages to prune. 
        /// </summary>
        /// <returns></returns>
        PrunePlan GeneratePrunePlan();
    }
}
