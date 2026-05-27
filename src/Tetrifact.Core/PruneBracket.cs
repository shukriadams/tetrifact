namespace Tetrifact.Core
{
    public class PruneBracket
    {
        #region PROPERTIES

        /// <summary>
        /// Number of packages to keep for the given period
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Days back in time that this bracket begins to apply. Starts relative to previous PruneBracket in setup.
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// minutes back in time that this bracket begins to apply. Starts relative to previous PruneBracket in setup.
        /// Note that practically days are the most likely actual bracket division, finer grain is more useful for testing.
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Hours back in time that this bracket begins to apply. Starts relative to previous PruneBracket in setup.
        /// Note that practically days are the most likely actual bracket division, finer grain is more useful for testing.
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PruneBracketGrouping Grouping { get; set; }

        #endregion

        #region CTORS

        public PruneBracket() 
        {
            Grouping = PruneBracketGrouping.Grouped;
        }

        #endregion

        #region METHODS

        public override string ToString()
        {
            return $"Covers {Days} day(s) ({Hours} hour(s), {Minutes} minutes(s)) back, allows {Amount} package(s), {Grouping}";
        }

        #endregion
    }
}
