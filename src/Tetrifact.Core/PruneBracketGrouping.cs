namespace Tetrifact.Core
{
    public enum PruneBracketGrouping
    {
        /// <summary>
        /// Default. Builds to keep spread over time to next (further back in time) bracket
        /// </summary>
        Grouped,

        /// <summary>
        /// Builds kept on day interval. Ie, for the given bracket, {Amount} of builds will be kept every day.
        /// </summary>
        Daily,

        /// <summary>
        /// Builds kept on weekly interval. Ie, for the given bracket, {Amount} of builds will be kept every week.
        /// </summary>
        Weekly, 

        /// <summary>
        /// Builds kept on monthly interval. Ie, for the given bracket, {Amount} of builds will be kept every month.
        /// </summary>
        Monthly,

        /// <summary>
        /// Builds kept on yearly interval. Ie, for the given bracket, {Amount} of builds will be kept every year.
        /// </summary>
        Yearly  
    }
}
