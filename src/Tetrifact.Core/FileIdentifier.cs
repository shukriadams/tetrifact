using System.Linq;
using System.Text.RegularExpressions;

namespace Tetrifact.Core
{
    /// <summary>
    /// Wraps public ID of a file. File ID consists of its path and hash, but is exposed as a uniform string to simplicity.
    /// Hash + path allows a file to be located in a package without having to provide the package id.
    /// </summary>
    public class FileIdentifier : IPackageFile
    {
        public string Path { get; set; }

        public string Hash { get; set; }

        /// <summary>
        /// Returns uniform string from Path+Hash
        /// </summary>
        /// <param name="path"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string Cloak(string path, string hash)
        {
            return Obfuscator.Cloak($"{path}::{hash}");
        }

        /// <summary>
        /// "Decrypts" uniform string into FileIdentifier instance exposing its path and hash.
        /// </summary>
        /// <param name="cloakedId"></param>
        /// <returns></returns>
        public static FileIdentifier Decloak(string cloakedId)
        {
            cloakedId = Obfuscator.Decloak(cloakedId);
            Regex regex = new Regex("(.*)::(.*)");
            MatchCollection matches = regex.Matches(cloakedId);

            if (matches.Count() != 1)
                throw new InvalidFileIdentifierException(cloakedId);

            return new FileIdentifier
            {
                Path = matches[0].Groups[1].Value,
                Hash = matches[0].Groups[2].Value
            };
        }
    }
}
