namespace Tetrifact.Core
{
    public class StringHelper
    {
        public static string ClipFromEnd(
            string main,
            int amount
            )
        {
            // returns blank string if invalid clip length given
            if (amount >= main.Length)
                return string.Empty;

            return main.Substring(
                0,
                main.Length - amount);
        }
    }
}
