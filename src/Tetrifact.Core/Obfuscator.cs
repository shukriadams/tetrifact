using System;
using System.Text;

namespace Tetrifact.Core
{
    /// <summary>
    /// Mangles/restores strings with base64. This is for cosmetic reasons (to mask complex data structure in public ids), and for sanitizing text for
    /// writing as filesystem names.
    /// </summary>
    public static class Obfuscator
    {
        public static string Cloak(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        public static string Decloak(string input)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(input));
            }
            catch (FormatException)
            {
                throw new InvalidFileIdentifierException(input);
            }
        }


    }
}
