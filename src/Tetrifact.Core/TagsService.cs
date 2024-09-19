using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Tetrifact.Core
{
    public class TagsService : ITagsService
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly ILogger<ITagsService> _log;

        private readonly IPackageListCache _packageListCache;

        private readonly IFileSystem _fileSystem;
        
        public static readonly string AllTagsCacheKey = "_allTagsCache";

        public static readonly string TagsThenPackagesKey = "_tagsThenPackageeCache";

        public static readonly string PackagesThenTagsKey = "_packagesThenTagsCache";

        private readonly IMemoryCache _cache;

        #endregion

        #region CTORS

        public TagsService(ISettings settings, IMemoryCache cache, IFileSystem fileSystem, ILogger<ITagsService> log, IPackageListCache packageListCache)
        {
            _settings = settings;
            _log = log;
            _packageListCache = packageListCache;
            _fileSystem = fileSystem;
            _cache = cache;
        }

        #endregion

        #region METHODS

        public void AddTag(string packageId, string tag)
        {
            string manifestPath = Path.Combine(_settings.PackagePath, packageId, "manifest.json");
            if (!_fileSystem.File.Exists(manifestPath))
                throw new PackageNotFoundException(packageId);

            // write tag to fs
            string targetFolder = Path.Combine(_settings.TagsPath, Obfuscator.Cloak(tag));
            _fileSystem.Directory.CreateDirectory(targetFolder);
            
            string targetPath = Path.Combine(targetFolder, packageId);
            if (!_fileSystem.File.Exists(targetPath))
                _fileSystem.File.WriteAllText(targetPath, string.Empty);

            // flush in-memory tags
            _packageListCache.Clear();
        }

        public void RemoveTag(string packageId, string tag)
        {
            string manifestPath = Path.Combine(_settings.PackagePath, packageId, "manifest.json");
            if (!_fileSystem.File.Exists(manifestPath))
                throw new PackageNotFoundException(packageId);

            string targetPath = Path.Combine(_settings.TagsPath, Obfuscator.Cloak(tag), packageId);
            if (_fileSystem.File.Exists(targetPath))
                _fileSystem.File.Delete(targetPath);

            // flush in-memory tags
            _packageListCache.Clear();
        }

        /// <summary>
        /// Gets a list of all tags from tag index folder. Tags are not read from package manifests. 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllTags()
        {
            List<string> tags;

            if (!_cache.TryGetValue(AllTagsCacheKey, out tags))
            {
                string[]  rawTags = _fileSystem.Directory.GetDirectories(_settings.TagsPath);
                tags = new List<string>();
                foreach (string rawTag in rawTags)
                {
                    try
                    {
                        tags.Add(Obfuscator.Decloak(Path.GetFileName(rawTag)));
                    }
                    catch (InvalidFileIdentifierException)
                    {
                        // log invalid tag folders, and continue.
                        _log.LogError($"The tag \"{rawTag}\" is not a valid base64 string. This node in the tags folder should be pruned out.");
                    }
                }

                // Set cache options.
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_settings.CacheTimeout));

                _cache.Set(AllTagsCacheKey, tags, cacheEntryOptions);
            }

            return tags;
        }

        /// <summary>
        /// Gets a list of all tags, and all packages tagged by each tag.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IEnumerable<string>> GetTagsThenPackages()
        {
            Dictionary<string, IEnumerable<string>> tags;

            if (!_cache.TryGetValue(TagsThenPackagesKey, out tags))
            {
                string[] rawTags = _fileSystem.Directory.GetDirectories(_settings.TagsPath);
                tags = new Dictionary<string, IEnumerable<string>>();

                foreach (string rawTag in rawTags)
                {
                    try
                    {
                        IEnumerable<string> packages = _fileSystem.Directory.GetFiles(rawTag).Select(r => Path.GetFileName(r));
                        tags.Add(Obfuscator.Decloak(Path.GetFileName(rawTag)), packages);
                    }
                    catch (InvalidFileIdentifierException)
                    {
                        // log invalid tag folders, and continue.
                        _log.LogError($"The tag \"{rawTag}\" is not a valid base64 string. This node in the tags folder should be pruned out.");
                    }
                }

                // Set cache options.
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_settings.CacheTimeout));

                _cache.Set(TagsThenPackagesKey, tags, cacheEntryOptions);
            }

            return tags;
        }

        /// <summary>
        /// Gets a list of all tags, and all packages tagged by each tag.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, IEnumerable<string>> GetPackagesThenTags()
        {
            Dictionary<string, IEnumerable<string>> packageDict;
            if (!_cache.TryGetValue(PackagesThenTagsKey, out packageDict))
            {
                packageDict = new Dictionary<string, IEnumerable<string>>();
                Dictionary<string, IEnumerable<string>> tags = this.GetTagsThenPackages();
                Dictionary<string, List<string>> packagetemp = new Dictionary<string, List<string>>();

                foreach (string tag in tags.Keys)
                    foreach (string package in tags[tag])
                    {
                        if (!packagetemp.ContainsKey(package))
                            packagetemp.Add(package, new List<string>());

                        packagetemp[package].Add(tag);
                    }

                foreach (string tag in packagetemp.Keys)
                    packageDict.Add(tag, packagetemp[tag]);

                // Set cache options.
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_settings.CacheTimeout));

                _cache.Set(PackagesThenTagsKey, tags, cacheEntryOptions);
            }

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
                if (!_fileSystem.Directory.Exists(tagDirectory))
                    throw new TagNotFoundException();

                string[] files = _fileSystem.Directory.GetFiles(tagDirectory);
                matches = matches.Union(files.Select(r => Path.GetFileName(r)));
            }

            return matches;
        }

        #endregion
    }
}
