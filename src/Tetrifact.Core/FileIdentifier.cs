﻿using System.Linq;
using System.Text.RegularExpressions;

namespace Tetrifact.Core
{
    public class FileIdentifier
    {
        public string Path { get; set; }

        public string Package { get; set; }

        public static string Cloak(string path, string hash)
        {
            return Obfuscator.Cloak($"{path}::{hash}");
        }

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
                Package = matches[0].Groups[2].Value
            };
        }
    }
}
