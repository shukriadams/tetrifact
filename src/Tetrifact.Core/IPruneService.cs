namespace Tetrifact.Core
{
    public interface IPruneService
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
