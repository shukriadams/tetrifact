namespace Tetrifact.Core
{
    public class PruneBracket
    {
        /// <summary>
        /// Number of packages to keep for the given period
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Days back in time that this bracket begins to apply. Starts relative to previous PruneBracket in setup.
        /// </summary>
        public int Days { get; set; }

        public PruneBracketGrouping Grouping { get; set; }

        public PruneBracket() 
        {
            Grouping = PruneBracketGrouping.Grouped;
        }

        public override string ToString()
        {
            return $"{Days} days {Amount} packages";
        }
    }
}
