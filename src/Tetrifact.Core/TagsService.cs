using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class TagsService : ITagsService
    {
        private ITetriSettings _settings;

        private ILogger<TagsService> _logger;

        public TagsService(ITetriSettings settings, ILogger<TagsService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public void AddTag(string packageId, string tag)
        {
            string manifestPath = Path.Combine(_settings.PackagePath, packageId, "manifest.json");
            if (!File.Exists(manifestPath))
                throw new PackageNotFoundException(packageId);

            // write tag to fs
            string targetFolder = Path.Combine(_settings.TagsPath, Obfuscator.Cloak(tag));
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            string targetPath = Path.Combine(targetFolder, packageId);
            if (!File.Exists(targetPath))
                File.Create(targetPath);


            // WARNING - NO FILE LOCK HERE!!
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));
            if (!manifest.Tags.Contains(tag))
            {
                manifest.Tags.Add(tag);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest));
            }

            /*
            using (FileStream fileStream = new FileStream(manifestPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                string fileContents = string.Empty;
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContents = reader.ReadToEnd();
                }

                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(fileContents);
                if (!manifest.Tags.Contains(tag))
                {
                    manifest.Tags.Add(tag);
                    byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(manifest));
                    fileStream.Write(data, 0, data.Length);
                }
            }
            */
        }

        public IEnumerable<string> GetTags()
        {
            string[] tags = Directory.GetDirectories(_settings.TagsPath);
            return tags.Select(r => Obfuscator.Decloak(Path.GetFileName(r)));
        }

        public IEnumerable<string> GetPackageIdsWithTag(string tag)
        {
            string tagDirectory = Path.Combine(_settings.TagsPath, Obfuscator.Cloak(tag));
            if (!Directory.Exists(tagDirectory))
                throw new TagNotFoundException();

            string[] files = Directory.GetFiles(tagDirectory);
            return files.Select(r => Path.GetFileName(r));
        }

        public void RemoveTag(string packageId, string tag)
        {
            string manifestPath = Path.Combine(_settings.PackagePath, packageId, "manifest.json");
            if (!File.Exists(manifestPath))
                throw new PackageNotFoundException(packageId);

            string targetPath = Path.Combine(_settings.TagsPath, Obfuscator.Cloak(tag), packageId);
            if (!File.Exists(targetPath))
                throw new TagNotFoundException();

            File.Delete(targetPath);

            // WARNING - NO FILE LOCK HERE!!
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));
            if (manifest.Tags.Contains(tag))
            {
                manifest.Tags.Remove(tag);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest));
            }
            /*
            using (FileStream fileStream = new FileStream(manifestPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                string fileContents = string.Empty;
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContents = reader.ReadToEnd();
                }

                Manifest manifest = JsonConvert.DeserializeObject<Manifest>(fileContents);
                if (manifest.Tags.Contains(tag))
                {
                    manifest.Tags.Remove(tag);
                    byte[] data = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(manifest));
                    fileStream.Write(data, 0, data.Length);
                }
            }
            */
        }
    }
}
