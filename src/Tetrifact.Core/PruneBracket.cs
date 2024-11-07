namespace Tetrifact
{
    public class PruneBracket
    {
        /// <summary>
        /// Number of builds to keep for the given period
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Days back in time that this bracket begins to appl
        /// </summary>
        public int Days { get; set; }

        public override string ToString()
        {
            return $"{Days} days {Amount} packages";
        }
    }
}
