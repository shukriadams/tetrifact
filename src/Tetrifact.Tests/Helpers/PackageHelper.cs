using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class PackageHelper
    {
        public static IEnumerable<string> GetManifestPaths(ISettings settings, string packageName)
        {
            return new string[]{ Path.Combine(settings.PackagePath, packageName, "manifest.json"), Path.Combine(settings.PackagePath, packageName, "manifest-head.json") };
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
        /// Creates a package with custom file content - use this to test complex packages with difference content. Files have fixed paths in package, iterated by position in content array.
        /// </summary>
        /// <returns>New package id</returns>
        public static string CreateNewPackage(ISettings settings, IEnumerable<string> filesContent)
        {
            IFileSystem filesystem = new FileSystem();
            ILockProvider lockProvider = new Core.LockProvider();
            IIndexReadService indexReader = new IndexReadService(
                settings, 
                new Core.TagsService(settings, filesystem, new TestLogger<ITagsService>(), new PackageListCache(MemoryCacheHelper.GetInstance())), 
                new TestLogger<IIndexReadService>(),
                filesystem, 
                HashServiceHelper.Instance(),
                lockProvider);

            IArchiveService archiveService = new Core.ArchiveService(
                indexReader, 
                new ThreadDefault(),
                lockProvider,
                filesystem, 
                new TestLogger<IArchiveService>(), 
                settings);

            IPackageCreateService PackageCreate = new PackageCreateService(
                indexReader,
                lockProvider,
                archiveService,
                settings,
                new TestLogger<IPackageCreateService>(),
                new PackageCreateWorkspace(settings, indexReader, filesystem, new TestLogger<IPackageCreateWorkspace>(), HashServiceHelper.Instance()),
                HashServiceHelper.Instance(),
                new TestFileSystem());

            List<PackageCreateItem> files = new List<PackageCreateItem>();
            string packageId = Guid.NewGuid().ToString();

            for (int i = 0; i < filesContent.Count(); i++)
            {
                string fileContent = filesContent.ElementAt(i);
                Stream fileContentStream = StreamsHelper.StreamFromString(fileContent);
                files.Add(new PackageCreateItem(fileContentStream, $"folder{i}/file{i}"));
            }

            PackageCreateArguments package = new PackageCreateArguments
            {
                Id = packageId,
                Files = files
            };

            PackageCreate.Create(package);
            return packageId;
        }

        /// <summary>
        /// Generates a single-file package, returns its unique id.
        /// </summary>
        /// <returns></returns>
        public static TestPackage CreateNewPackage(ISettings settings)
        {
            return CreateNewPackageFiles(settings, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Creates a package with some file data. 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static TestPackage CreateNewPackageFiles(ISettings settings, string packageName)
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

            //IPackageCreateWorkspace workspace = NinjectHelper.Get<IPackageCreateWorkspace>("settings", settings, "indexReadService", indexReader2); // new PackageCreateWorkspace(settings, indexReader, new FileSystem(), new TestLogger<IPackageCreateWorkspace>(), HashServiceHelper.Instance());
            IPackageCreateWorkspace workspace = NinjectHelper.Get<IPackageCreateWorkspace>(settings);
            workspace.Initialize();
            workspace.AddIncomingFile(StreamsHelper.StreamFromBytes(testPackage.Content), testPackage.Path);
            workspace.WriteFile(testPackage.Path, testPackage.Hash, testPackage.Content.Length, testPackage.Id);
            workspace.WriteManifest(testPackage.Id, HashServiceHelper.Instance().FromString(filePathHash+ testPackage.Hash));

            return testPackage;
        }

        /// <summary>
        /// Creates an in-memory Manifest representation of a package, with random content. Manifest id + paths are random
        /// </summary>
        /// <returns></returns>
        public static Manifest CreateInMemoryManifest()
        { 
            string[] files = new string[new Random().Next(10, 20)];
            for (int i = 0 ; i < files.Length ; i ++)
                files[i] = Guid.NewGuid().ToString();

            return CreateInMemoryManifest(files);
        }

        /// <summary>
        /// Creates an in-memory Manifest representation of a package, from file content. Manifest id + paths are random
        /// </summary>
        /// <param name="filesContent"></param>
        /// <returns></returns>
        public static Manifest CreateInMemoryManifest(IEnumerable<string> filesContent)
        { 
            List<ManifestItem> items = new List<ManifestItem>();

            foreach (string fileContent in filesContent)
            { 
                string contentHash = HashServiceHelper.Instance().FromString(fileContent);
                string path = $"{Guid.NewGuid()}/{Guid.NewGuid()}";
                items.Add(new ManifestItem{ 
                    Id = Obfuscator.Cloak(path + contentHash),
                    Hash = contentHash,
                    Path = path 
                });
            }

            Manifest manifest = new Manifest{ 
                Id = Guid.NewGuid().ToString(),
                CreatedUtc = DateTime.UtcNow,
                Files = items
            };

            return manifest;
        }
    }
}
