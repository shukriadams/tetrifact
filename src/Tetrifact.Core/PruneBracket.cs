namespace Tetrifact
{
    public class PruneBracket
    {
        public int Amount { get; set; }

        public int Days { get; set; }

        public override string ToString()
        {
            return $"{Days} days {Amount} packages";
        }
    }
}
