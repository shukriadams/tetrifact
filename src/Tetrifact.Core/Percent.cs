namespace Tetrifact.Core
{
    // 
    public class Percent
    {
        /// <summary>
        /// Force cast all numeric to decimal
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static int Calc(decimal first, decimal second)
        {
            // overflow check
            if (second == 0)
                return 0;

            decimal p = first / second;
            return (int)System.Math.Round((decimal)(p * 100), 0);
        }

        /// <summary>
        /// Force cast to double
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static int Calc(double first, double second)
        {
            // overflow check
            if (second == 0)
                return 0;

            double p = first / second;
            return (int)System.Math.Round((double)(p * 100), 0);
        }

        public static int Calc(long first, long second)
        {
            // overflow check
            if (second == 0)
                return 0;

            double p = (double)first / (double)second;
            return (int)System.Math.Round((double)(p * 100), 0);
        }
    }
}
