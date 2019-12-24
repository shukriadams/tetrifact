using System;

namespace Tetrifact.Dev
{
    public class DataHelper
    {
        /// <summary>
        /// Generates an array of random data with variable size
        /// </summary>
        /// <param name="fromKb"></param>
        /// <param name="toKb"></param>
        /// <returns></returns>
        public static byte[] GetRandomData(int fromKb, int toKb)
        {
            Random rnd = new Random();
            int kb = rnd.Next(fromKb, toKb);
            return GetRandomData(kb);
        }

        /// <summary>
        /// Generates a byte array of random data
        /// </summary>
        /// <param name="kb"></param>
        /// <returns></returns>
        public static byte[] GetRandomData(int kb)
        {
            Random rnd = new Random();
            Byte[] b = new Byte[kb * 1024];
            rnd.NextBytes(b);
            return b;
        }
    }
}
