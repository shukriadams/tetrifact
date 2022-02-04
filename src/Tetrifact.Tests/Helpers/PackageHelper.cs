using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class PackageHelper
    {
        public static string GetManifestPath(ISettings settings, string packageName)
        {
            return Path.Combine(settings.PackagePath, packageName, "manifest.json");
        }

        /// <summary>
        /// Writes a manifest directly. does not generate files. Use this for manifest tests only where you need control of the manifest.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="manifest"></param>
        public static void WriteManifest(ISettings settings, Manifest manifest)
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
        public static TestPackage CreatePackage(ISettings settings)
        {
            return CreatePackage(settings, "somepackage");
        }

        /// <summary>
        /// Creates a package with some file data. 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static TestPackage CreatePackage(ISettings settings, string packageName)
        {
            // create package, files folder and item location in one
            byte[] content = Encoding.ASCII.GetBytes("some content");
            TestPackage testPackage = new TestPackage
            {
                Content = content,
                Path = $"path/to/{packageName}",
                Hash = HashServiceHelper.Instance().FromByteArray(content),
                Id = packageName
            };

            string filePathHash = HashServiceHelper.Instance().FromString(testPackage.Path);

            // create via workspace writer. Note that workspace has no logic of its own to handle hashing, it relies on whatever
            // calls it to do that. We could use PackageCreate to do this, but as we want to test PackageCreate with this helper
            // we keep this as low-level as possible
            IWorkspace workspace = new Core.Workspace(settings, new TestLogger<IWorkspace>(), HashServiceHelper.Instance());
            workspace.Initialize();
            workspace.AddIncomingFile(StreamsHelper.StreamFromBytes(testPackage.Content), testPackage.Path);
            workspace.WriteFile(testPackage.Path, testPackage.Hash, testPackage.Content.Length, testPackage.Id);
            workspace.WriteManifest(testPackage.Id, HashServiceHelper.Instance().FromString(filePathHash+ testPackage.Hash));

            return testPackage;
        }
    }
}
