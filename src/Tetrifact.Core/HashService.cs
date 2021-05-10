using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tetrifact.Core
{
    /// <summary>
    /// SHA256 hashing
    /// </summary>
    public class HashService : IHashService
    {

        /// <summary>
        /// Locally utilty function, hex stage of generating hash.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string ToHex(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();

            foreach (byte b in bytes)
                s.Append(b.ToString("x2").ToLower());

            return s.ToString();
        }


        /// <summary>
        /// Generates a SHA256 hash of the file at the given path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string FromFile(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(fs);
                return ToHex(hash);
            }
        }


        /// <summary>
        /// Generates a SHA256 hash from a byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string FromByteArray(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(stream);
                return ToHex(hash);
            }
        }


        /// <summary>
        /// Generates a SHA256 hash from a string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string FromString(string str)
        {
            Stream stream = StreamsHelper.StreamFromString(str);
            using (HashAlgorithm hashAlgorithm = SHA256.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(stream);
                return ToHex(hash);
            }
        }


        /// <summary>
        /// Sorts file paths so they are in standard order for hash creation.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public string[] SortFileArrayForHashing(string[] files)
        {
            Array.Sort(files, (x, y) => String.Compare(x, y));
            return files;
        }

    }
}
