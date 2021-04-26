using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class PackageHelper
    {
        public static void WritePackage(ITetriSettings settings, Manifest manifest)
        {
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, manifest.Id));
            File.WriteAllText(Path.Combine(settings.PackagePath, manifest.Id, "manifest.json"), JsonConvert.SerializeObject(manifest));

            Manifest headCopy = JsonConvert.DeserializeObject<Manifest>(JsonConvert.SerializeObject(manifest));
            headCopy.Tags = new HashSet<string>();
            File.WriteAllText(Path.Combine(settings.PackagePath, manifest.Id, "manifest-head.json"), JsonConvert.SerializeObject(headCopy));
        }
    }
}
