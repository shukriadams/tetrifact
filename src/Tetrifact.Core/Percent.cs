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
            double p = first / second;
            return (int)System.Math.Round((double)(p * 100), 0);
        }
    }
}
