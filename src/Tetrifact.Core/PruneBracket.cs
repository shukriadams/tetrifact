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
            return $"Covers {Days} day(s), allows {Amount} package(s), {Grouping}";
        }

        #endregion
    }
}
