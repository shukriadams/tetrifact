using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    /// <summary>
    /// Package list logic ; implements in-memory caching to save on expensive read operations, as generating a list of packages requires 
    /// loading the JSON manifest of each package. 
    /// </summary>
    public class PackageList : IPackageList
    {
        #region FIELDS

        private readonly IMemoryCache _cache;

        private readonly ITetriSettings _settings;

        private readonly ILogger<IPackageList> _logger;

        

        private readonly IIndexReader _indexReader;

        #endregion

        #region CTORS

        public PackageList(IMemoryCache memoryCache, IIndexReader indexReader, ITetriSettings settings, ILogger<IPackageList> logger)
        {
            _cache = memoryCache;
            _settings = settings;
            _logger = logger;
            _indexReader = indexReader;
        }

        #endregion

        #region METHODS

        private static string GetCacheKey(string project) 
        {
            return $"${project}_packageCache";
        }

        public IEnumerable<string> GetAllTags(string project) 
        {
            IList<Package> packageData = null;
            if (!_cache.TryGetValue(GetCacheKey(project), out packageData))
                packageData = this.GeneratePackageData(project);

            return packageData.Select(r => r.Tags.ToList()).SelectMany(r => r).Distinct();
        }

        public IEnumerable<string> GetPackagesWithTag(string project, string tag) 
        {
            IList<Package> packageData = null;
            if (!_cache.TryGetValue(GetCacheKey(project), out packageData))
                packageData = this.GeneratePackageData(project);

            return packageData.Where(r => r.Tags.Contains(tag)).Select(r => r.Id);
        }

        public void Clear(string project)
        {
            _cache.Remove(GetCacheKey(project));
        }

        public IEnumerable<string> GetPopularTags(string project, int count)
        {
            if (string.IsNullOrEmpty(project))
                return new List<string>();

            IList<Package> packageData = null;

            if (!_cache.TryGetValue(GetCacheKey(project), out packageData))
                packageData = this.GeneratePackageData(project);

            Dictionary<string, int> tags = new Dictionary<string, int>();
            foreach (Package package in packageData)
            {
                foreach (string tag in package.Tags)
                {
                    if (!tags.ContainsKey(tag))
                        tags.Add(tag, 0);

                    tags[tag]++;
                }
            }

            return tags.OrderByDescending(r => r.Value).Take(count).Select(r => r.Key);
        }

        public IEnumerable<Package> GetWithTag(string project, string tag, int pageIndex, int pageSize)
        {
            IList<Package> packageData = null;

            if (!_cache.TryGetValue(GetCacheKey(project), out packageData))
            {
                packageData = this.GeneratePackageData(project);
            }

            return packageData.Where(r => r.Tags.Contains(tag)).Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<Package> Get(string project, int pageIndex, int pageSize)
        {
            if (string.IsNullOrEmpty(project))
                return new List<Package>();

            IList<Package> packageData;
            
            
            if (!_cache.TryGetValue(GetCacheKey(project), out packageData))
            {
                packageData = this.GeneratePackageData(project);
            }

            return packageData.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public PageableData<Package> GetPage(string project, int pageIndex, int pageSize)
        {
            IList<Package> packageData;


            if (!_cache.TryGetValue(GetCacheKey(project), out packageData))
            {
                packageData = this.GeneratePackageData(project);
            }

            return new PageableData<Package>(packageData.Skip(pageIndex * pageSize).Take(pageSize), pageIndex, pageSize, packageData.Count);
        }

        public Package GetLatestWithTag(string project, string tag)
        {
            IList<Package> packageData;

            if (!_cache.TryGetValue(GetCacheKey(project), out packageData))
            {
                packageData = this.GeneratePackageData(project);
            }

            return packageData.Where(r => r.Tags.Contains(tag)).OrderByDescending(r => r.CreatedUtc).FirstOrDefault();
        }

        #endregion

        #region METHODS Private

        /// <summary>
        /// Generates an in-memory list of all packages in a given project. In-memory caching saves reads from the
        /// filesystem.
        /// </summary>
        /// <returns></returns>
        private IList<Package> GeneratePackageData(string project)
        {
            IList<Package> packageData = new List<Package>();
            IEnumerable<string> manifestPaths = _indexReader.GetManifestPaths(project);

            foreach (string manifestPath in manifestPaths)
            {
                try
                {
                    Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment, manifestPath)));
                    packageData.Add(new Package
                    {
                        CreatedUtc = manifest.CreatedUtc,
                        Id = manifest.Id,
                        Description = manifest.Description,
                        Hash = manifest.Hash,
                        Tags = manifest.Tags
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error trying to reading manifest @ {manifestPath}");
                }
            }

            packageData = packageData.OrderByDescending(r => r.CreatedUtc).ToList();

            // Set cache options.
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(_settings.CacheTimeout));

            _cache.Set(GetCacheKey(project), packageData);

            return packageData;
        }

        #endregion
    }
}
