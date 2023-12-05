using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Tetrifact.Core
{

    /// <summary>
    /// Package list logic ; implements in-memory caching to save on expensive read operations, as generating a list of packages requires 
    /// loading the JSON manifest of each package. 
    /// </summary>
    public class PackageListService : IPackageListService
    {
        #region FIELDS

        private readonly IMemoryCache _cache;

        private readonly IFileSystem _fileSystem;

        private readonly ISettings _settings;

        private readonly ILogger<IPackageListService> _logger;

        private readonly ITagsService _tagService;

        public static readonly string CacheKey = "_packageCache";

        #endregion

        #region CTORS

        public PackageListService(IMemoryCache memoryCache, ISettings settings, ITagsService tagService, IFileSystem fileSystem, ILogger<IPackageListService> logger)
        {
            _cache = memoryCache;
            _settings = settings;
            _tagService = tagService;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        #endregion

        #region METHODS

        public IEnumerable<string> GetPopularTags(int count)
        {
            Dictionary<string, int> tags = new Dictionary<string, int>();
            Dictionary<string, IEnumerable<string>> tagsThenPackages = _tagService.GetTagsThenPackages();

            foreach (string tag in tagsThenPackages.Keys)
                tags[tag] = tagsThenPackages[tag].Count();

            return tags
                .OrderByDescending(r => r.Value)
                .Where(r => r.Value > 1) //ignore any tag with only 1 use from "popular" list
                .Take(count)
                .Select(r => r.Key);
        }

        public IEnumerable<Package> GetWithTags(string[] tags, int pageIndex, int pageSize)
        {
            IList<Package> packageData = null;

            if (!_cache.TryGetValue(CacheKey, out packageData))
            {
                packageData = this.GeneratePackageData();
            }
            
            return packageData.Where(r => tags.IsSubsetOf(r.Tags)).Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<Package> Get(int pageIndex, int pageSize)
        {
            IList<Package> packageData;
            
            if (!_cache.TryGetValue(CacheKey, out packageData))
            {
                packageData = this.GeneratePackageData();
            }

            return packageData.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public PageableData<Package> GetPage(int pageIndex, int pageSize)
        {
            IList<Package> packageData;


            if (!_cache.TryGetValue(CacheKey, out packageData))
            {
                packageData = this.GeneratePackageData();
            }

            return new PageableData<Package>(packageData.Skip(pageIndex * pageSize).Take(pageSize), pageIndex, pageSize, packageData.Count);
        }

        public Package GetLatestWithTags(string[] tags)
        {
            IList<Package> packageData;

            if (!_cache.TryGetValue(CacheKey, out packageData))
            {
                packageData = this.GeneratePackageData();
            }

            return packageData.Where(r => tags.IsSubsetOf(r.Tags)).OrderByDescending(r => r.CreatedUtc).FirstOrDefault();
        }

        #endregion

        #region METHODS Private

        private IList<Package> GeneratePackageData()
        {
            IList<Package> packageData = new List<Package>();
            IDirectoryInfo dirInfo = this._fileSystem.DirectoryInfo.FromDirectoryName(_settings.PackagePath);
            IEnumerable<string> packageDirectories = dirInfo.EnumerateDirectories().Select(d => d.FullName);
            Dictionary<string, IEnumerable<string>> packagesThenTags = _tagService.GetPackagesThenTags();

            foreach (string packageDirectory in packageDirectories)
            {
                try
                {
                    // generate manifest head if it doesn't exist
                    string manifestHeadPath = Path.Join(packageDirectory, "manifest-head.json");
                    if (!File.Exists(manifestHeadPath))
                    {
                        string manifestPath = Path.Join(packageDirectory, "manifest.json");
                        if (!_fileSystem.File.Exists(manifestPath))
                            continue;

                        Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestPath));
                        manifest.Files = new List<ManifestItem>();
                        File.WriteAllText(manifestHeadPath, JsonConvert.SerializeObject(manifest));
                    }

                    Manifest manifestHead = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifestHeadPath));
                    string packageId = Path.GetFileName(packageDirectory);
                    packageData.Add(new Package
                    {
                        CreatedUtc = manifestHead.CreatedUtc,
                        Id = packageId,
                        Description = manifestHead.Description,
                        Hash = manifestHead.Hash,
                        Tags = packagesThenTags.ContainsKey(packageId) ? packagesThenTags[packageId].ToHashSet() : new HashSet<string>()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error trying to reading manifest @ {packageDirectory}");
                }
            }

            packageData = packageData.OrderByDescending(r => r.CreatedUtc).ToList();

            // Set cache options.
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_settings.CacheTimeout));

            _cache.Set(CacheKey, packageData);

            return packageData;
        }

        #endregion
    }
}
