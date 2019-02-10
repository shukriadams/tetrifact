using System;
using System.Text;

namespace Tetrifact.Core
{
    public static class Obfuscator
    {
        public  static string Cloak(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        public static string Decloak(string input)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }
    }
}
