using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class TagHelper
    {
        public static void TagPackage(ITetriSettings settings, string tag, string packageId)
        {
            Directory.CreateDirectory(Path.Combine(settings.TagsPath, Obfuscator.Cloak(tag)));
            File.WriteAllText(Path.Combine(settings.TagsPath, Obfuscator.Cloak(tag), packageId), string.Empty);

        }

    }

}
