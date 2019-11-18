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

        private readonly string _cacheKey = "_packageCache";

        #endregion

        #region CTORS

        public PackageList(IMemoryCache memoryCache, ITetriSettings settings, ILogger<IPackageList> logger)
        {
            _cache = memoryCache;
            _settings = settings;
            _logger = logger;
        }

        #endregion

        #region METHODS

        public void Clear()
        {
            _cache.Remove("_packageCache");
        }

        public IEnumerable<string> GetPopularTags(string project, int count)
        {
            if (string.IsNullOrEmpty(project))
                return new List<string>();

            IList<Package> packageData = null;

            if (!_cache.TryGetValue(_cacheKey, out packageData))
            {
                packageData = this.GeneratePackageData(project);
            }

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

            if (!_cache.TryGetValue(_cacheKey, out packageData))
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
            
            
            if (!_cache.TryGetValue(_cacheKey, out packageData))
            {
                packageData = this.GeneratePackageData(project);
            }

            return packageData.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public PageableData<Package> GetPage(string project, int pageIndex, int pageSize)
        {
            IList<Package> packageData;


            if (!_cache.TryGetValue(_cacheKey, out packageData))
            {
                packageData = this.GeneratePackageData(project);
            }

            return new PageableData<Package>(packageData.Skip(pageIndex * pageSize).Take(pageSize), pageIndex, pageSize, packageData.Count);
        }

        public Package GetLatestWithTag(string project, string tag)
        {
            IList<Package> packageData;

            if (!_cache.TryGetValue(_cacheKey, out packageData))
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
            string packagePath = PathHelper.GetExpectedPackagesPath(_settings, project);
            DirectoryInfo dirInfo = new DirectoryInfo(packagePath);
            IEnumerable<string> packageDirectories = dirInfo.EnumerateDirectories().Select(d => d.FullName);

            foreach (string packageDirectory in packageDirectories)
            {
                try
                {
                    Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Join(packageDirectory, "manifest.json")));
                    packageData.Add(new Package
                    {
                        CreatedUtc = manifest.CreatedUtc,
                        Id = Path.GetFileName(packageDirectory),
                        Description = manifest.Description,
                        Hash = manifest.Hash,
                        Tags = manifest.Tags
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

            _cache.Set(_cacheKey, packageData);

            return packageData;
        }

        #endregion
    }
}
