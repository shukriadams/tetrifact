using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class TagsService : ITagsService
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<ITagsService> _logger;

        private readonly IPackageListCache _packageListCache;

        #endregion

        #region CTORS

        public TagsService(ISettings settings, ILogger<ITagsService> logger, IPackageListCache packageListCache)
        {
            _settings = settings;
            _logger = logger;
            _packageListCache = packageListCache;
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
            Directory.CreateDirectory(targetFolder);
            
            string targetPath = Path.Combine(targetFolder, packageId);
            if (!File.Exists(targetPath))
                File.WriteAllText(targetPath, string.Empty);

            // flush in-memory tags
            _packageListCache.Clear();
        }

        public void RemoveTag(string packageId, string tag)
        {
            string manifestPath = Path.Combine(_settings.PackagePath, packageId, "manifest.json");
            if (!File.Exists(manifestPath))
                throw new PackageNotFoundException(packageId);

            string targetPath = Path.Combine(_settings.TagsPath, Obfuscator.Cloak(tag), packageId);
            if (File.Exists(targetPath))
                File.Delete(targetPath);

            // flush in-memory tags
            _packageListCache.Clear();
        }

        /// <summary>
        /// Gets a list of all tags from tag index folder. Tags are not read from package manifests. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTags()
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

        /// <summary>
        /// Gets a list of all tags, and all packages tagged by each tag.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IEnumerable<string>> GetTagsThenPackages()
        {
            string[] rawTags = Directory.GetDirectories(_settings.TagsPath);
            Dictionary<string, IEnumerable<string>> tags = new Dictionary<string, IEnumerable<string>>();

            foreach (string rawTag in rawTags)
            {
                try
                {
                    IEnumerable<string> packages = Directory.GetFiles(rawTag).Select(r => Path.GetFileName(r));
                    tags.Add(Obfuscator.Decloak(Path.GetFileName(rawTag)), packages);
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
        /// Gets a list of all tags, and all packages tagged by each tag.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IEnumerable<string>> GetPackagesThenTags()
        {
            Dictionary<string, IEnumerable<string>> tags = this.GetTagsThenPackages();

            Dictionary<string, List<string>> packagetemp = new Dictionary<string, List<string>>();

            foreach (string tag in tags.Keys)
                foreach(string package in tags[tag])
                {
                    if (!packagetemp.ContainsKey(package))
                        packagetemp.Add(package, new List<string>());

                    packagetemp[package].Add(tag);
                }

            Dictionary<string, IEnumerable<string>> packageDict = new Dictionary<string, IEnumerable<string>>();
            foreach (string tag in packagetemp.Keys)
                packageDict.Add(tag, packagetemp[tag]);

            return packageDict;
        }

        /// <summary>
        /// Gets packages with tags from the tag index.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public IEnumerable<string> GetPackageIdsWithTags(string[] tags)
        {
            IEnumerable<string> matches = new List<string>();

            foreach (string tag in tags) {
                string tagDirectory = Path.Combine(_settings.TagsPath, Obfuscator.Cloak(tag));
                if (!Directory.Exists(tagDirectory))
                    throw new TagNotFoundException();

                string[] files = Directory.GetFiles(tagDirectory);
                matches = matches.Union(files.Select(r => Path.GetFileName(r)));
            }

            return matches;
        }

        #endregion
    }
}
