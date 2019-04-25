using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class TagsService : ITagsService
    {
        #region FIELDS

        private ITetriSettings _settings;

        private ILogger<ITagsService> _logger;

        private IPackageList _packageList;

        #endregion

        #region CTORS

        public TagsService(ITetriSettings settings, ILogger<ITagsService> logger, IPackageList packageList)
        {
            _settings = settings;
            _logger = logger;
            _packageList = packageList;
        }

        #endregion

        #region METHODS

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
                File.WriteAllText(targetPath, string.Empty);
                

            // WARNING - NO FILE LOCK HERE!!
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));
            if (!manifest.Tags.Contains(tag))
            {
                manifest.Tags.Add(tag);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest));
            }

            // flush in-memory tags
            _packageList.Clear();

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


        /// <summary>
        /// Gets tags from /tags index folder  (not directly from individual packages). 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReadTagsFromIndex()
        {
            string[] rawTags = Directory.GetDirectories(_settings.TagsPath);
            List<string> tags = new List<string>();

            foreach (string rawTag in rawTags)
            {
                try
                {
                    tags.Add(Obfuscator.Decloak(Path.GetFileName(rawTag)));
                }
                catch (FormatException)
                {
                    // log invalid tag folders, and continue.
                    _logger.LogError($"The tag \"{rawTag}\" is not a valid base64 string. This node in the tags folder should be pruned out.");
                }
            }

            return tags;
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
            if (File.Exists(targetPath))
                File.Delete(targetPath);

            // WARNING - NO FILE LOCK HERE!!
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));
            if (manifest.Tags.Contains(tag))
            {
                manifest.Tags.Remove(tag);
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest));
            }

            // flush in-memory tags
            _packageList.Clear();

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

        #endregion
    }
}
