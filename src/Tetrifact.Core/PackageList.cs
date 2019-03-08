using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class PackageList
    {
        private IMemoryCache _cache;

        private ITetriSettings _settings;

        private ILogger<PackageList> _logger;

        readonly string _cacheKey = "_packageCache";

        public PackageList(IMemoryCache memoryCache, ITetriSettings settings, ILogger<PackageList> logger)
        {
            _cache = memoryCache;
            _settings = settings;
            _logger = logger;
        }

        public void Clear()
        {
            _cache.Remove("_packageCache");
        }

        private IList<Package> Generate()
        {
            IList<Package> packageData = new List<Package>();

            DirectoryInfo dirInfo = new DirectoryInfo(_settings.PackagePath);
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

        public IEnumerable<Package> GetWithTag(string tag, int pageIndex, int pageSize)
        {
            IList<Package> packageData;


            if (!_cache.TryGetValue(_cacheKey, out packageData))
            {
                packageData = this.Generate();
            }

            return packageData.Where(r => r.Tags.Contains(tag)).Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<Package> Get(int pageIndex, int pageSize)
        {
            IList<Package> packageData;
            
            
            if (!_cache.TryGetValue(_cacheKey, out packageData))
            {
                packageData = this.Generate();
            }

            return packageData.Skip(pageIndex * pageSize).Take(pageSize);
        }

    }
}
