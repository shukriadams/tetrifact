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

        private readonly ITetriSettings _settings;

        private readonly ILogger<ITagsService> _logger;

        private readonly IPackageList _packageList;

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

        public void AddTag(string project, string packageId, string tag)
        {
            string manifestPath = this.GetManifestPath(project, packageId);
            if (!File.Exists(manifestPath))
                throw new PackageNotFoundException(packageId);

            // write tag to fs
            string targetFolder = Path.Combine(GetExpectedTagsPath(project), Obfuscator.Cloak(tag));
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

        public void RemoveTag(string project, string packageId, string tag)
        {
            string manifestPath = this.GetManifestPath(project, packageId);
            if (!File.Exists(manifestPath))
                throw new PackageNotFoundException(packageId);

            string targetPath = Path.Combine(GetExpectedTagsPath(project), Obfuscator.Cloak(tag), packageId);
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

        /// <summary>
        /// Gets a list of all tags from tag index folder. Tags are not read from package manifests. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTags(string project)
        {
            string tagsPath = this.GetExpectedTagsPath(project);
            string[] rawTags = Directory.GetDirectories(tagsPath);
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

        /// <summary>
        /// Gets packages with tag from the tag index, not directly from package manifests.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public IEnumerable<string> GetPackagesWithTag(string project, string tag)
        {
            string tagsPath = this.GetExpectedTagsPath(project);
            string tagDirectory = Path.Combine(tagsPath, Obfuscator.Cloak(tag));
            if (!Directory.Exists(tagDirectory))
                throw new TagNotFoundException();

            string[] files = Directory.GetFiles(tagDirectory);
            return files.Select(r => Path.GetFileName(r));
        }

        private string GetExpectedTagsPath(string project) 
        {
            string tagsPath = Path.Combine(_settings.ProjectsPath, project, Constants.TagsFragment);
            if (!Directory.Exists(tagsPath))
                throw new ProjectNotFoundException(project);

            return tagsPath;
        }

        private string GetManifestPath(string project, string package) 
        {
            return Path.Combine(_settings.ProjectsPath, project, Constants.PackagesFragment, package, "manifest.json");
        }

        #endregion
    }
}
