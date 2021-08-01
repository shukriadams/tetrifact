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

            // create via workspace writer. Note that workspace has no logic of its own to handle hashing, it relies on whatever
            // calls it to do that. We could use PackageCreate to do this, but as we want to test PackageCreate with this helper
            // we keep this as low-level as possible
            IWorkspace workspace = new Core.Workspace(settings, new TestLogger<IWorkspace>(), HashServiceHelper.Instance());
            workspace.Initialize();
            workspace.AddIncomingFile(StreamsHelper.StreamFromBytes(testPackage.Content), testPackage.Path);
            string fileHash = HashServiceHelper.Instance().FromByteArray(testPackage.Content);
            string filePathHash = HashServiceHelper.Instance().FromString(testPackage.Path);
            workspace.WriteFile(testPackage.Path, fileHash, testPackage.Content.Length, testPackage.Name);
            workspace.WriteManifest(testPackage.Name, HashServiceHelper.Instance().FromString(filePathHash+fileHash));

            return testPackage;
        }
    }
}
