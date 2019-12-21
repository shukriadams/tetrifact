using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tetrifact.Core
{
    /// <summary>
    /// Hashes various input date into SHA256.
    /// </summary>
    public class HashService
    {
        /// <summary>
        /// Internal method for hexing
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string ToHex(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();
            foreach (byte b in bytes)
                s.Append(b.ToString("x2").ToLower());

            return s.ToString();
        }

        public static string FromFile(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(fs);
                return ToHex(hash);
            }
        }

        public static string FromByteArray(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(stream);
                return ToHex(hash);
            }
        }

        public static string FromString(string str)
        {
            Stream stream = StreamsHelper.StreamFromString(str);
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(stream);
                return ToHex(hash);
            }
        }

    }
}
