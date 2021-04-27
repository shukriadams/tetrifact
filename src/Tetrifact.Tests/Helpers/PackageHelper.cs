using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class PackageHelper
    {
        /// <summary>
        /// Writes a manifest directly. does not generate files. Use this for manifest tests only where you need control of the manifest.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="manifest"></param>
        public static void WriteManifest(ITetriSettings settings, Manifest manifest)
        {
            Directory.CreateDirectory(Path.Combine(settings.PackagePath, manifest.Id));
            File.WriteAllText(Path.Combine(settings.PackagePath, manifest.Id, "manifest.json"), JsonConvert.SerializeObject(manifest));

            // create directly
            Manifest headCopy = JsonConvert.DeserializeObject<Manifest>(JsonConvert.SerializeObject(manifest));
            headCopy.Tags = new HashSet<string>();
            File.WriteAllText(Path.Combine(settings.PackagePath, manifest.Id, "manifest-head.json"), JsonConvert.SerializeObject(headCopy));
        }

        /// <summary>
        /// Generates a valid package, returns its unique id.
        /// </summary>
        /// <returns></returns>
        public static TestPackage CreatePackage(ITetriSettings settings)
        {
            return CreatePackage(settings, "somepackage");
        }

        /// <summary>
        /// Creates a packge with some file data. 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static TestPackage CreatePackage(ITetriSettings settings, string packageName)
        {
            // create package, files folder and item location in one
            TestPackage testPackage = new TestPackage
            {
                Content = Encoding.ASCII.GetBytes("some content"),
                Path = $"path/to/{packageName}",
                Name = packageName
            };

            // create via workspace writter
            IWorkspace workspace = new Core.Workspace(settings, new TestLogger<IWorkspace>());
            workspace.Initialize();
            workspace.AddIncomingFile(StreamsHelper.StreamFromBytes(testPackage.Content), testPackage.Path);
            workspace.WriteFile(testPackage.Path, "somehash", testPackage.Name);
            workspace.WriteManifest(testPackage.Name, "somehash2");

            return testPackage;
        }
    }
}
