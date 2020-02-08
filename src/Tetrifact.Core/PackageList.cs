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
        private class CachedData 
        {
            public IEnumerable<Package> Packages { get; set; }

            public IEnumerable<string> Projects { get; set; }
        }

        #region FIELDS

        private readonly IMemoryCache _cache;

        private readonly ILogger<IPackageList> _logger;

        private readonly IIndexReader _indexReader;

        #endregion

        #region CTORS

        public PackageList(IMemoryCache memoryCache, IIndexReader indexReader, ILogger<IPackageList> logger)
        {
            _cache = memoryCache;
            _logger = logger;
            _indexReader = indexReader;
        }

        #endregion

        #region METHODS

        private static string GetProjectCacheKey() 
        {
            return $"_projectsCache";
        }

        private static string GetPackageCacheKey(string project) 
        {
            return $"{project}_packageCache";
        }

        public IEnumerable<string> GetAllTags(string project) 
        {
            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            return packages.Select(r => r.Tags.ToList()).SelectMany(r => r).Distinct();
        }

        public IEnumerable<string> GetPackageIds(string project, int pageIndex, int pageSize)
        {
            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            return packages.OrderBy(r => r.Id).Skip(pageIndex).Take(pageSize).Select(r => r.Id);
        }

        public IEnumerable<string> GetPackagesWithTag(string project, string tag) 
        {
            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            return packages.Where(r => r.Tags.Contains(tag)).Select(r => r.Id);
        }

        public void Clear(string project)
        {
            _cache.Remove(GetPackageCacheKey(project));
        }

        public void Clear()
        {
            _cache.Remove(GetProjectCacheKey());
        }

        public IEnumerable<string> GetPopularTags(string project, int count)
        {
            if (string.IsNullOrEmpty(project))
                return new List<string>();

            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            Dictionary<string, int> tags = new Dictionary<string, int>();
            foreach (Package package in packages)
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
            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            return packages.Where(r => r.Tags.Contains(tag)).Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<Package> Get(string project, int pageIndex, int pageSize)
        {
            if (string.IsNullOrEmpty(project))
                return new List<Package>();

            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            return packages.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public PageableData<Package> GetPage(string project, int pageIndex, int pageSize)
        {
            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            return new PageableData<Package>(packages.Skip(pageIndex * pageSize).Take(pageSize), pageIndex, pageSize, packages.Count);
        }

        public IEnumerable<Package> GetUndiffedPackages(string project)
        {
            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            return packages.Where(r => !r.IsDiffed);
        }

        public Package GetLatestWithTag(string project, string tag)
        {
            IList<Package> packages;
            if (!_cache.TryGetValue(GetPackageCacheKey(project), out packages))
                packages = this.GeneratePackageData(project);

            return packages.Where(r => r.Tags.Contains(tag)).OrderByDescending(r => r.CreatedUtc).FirstOrDefault();
        }

        public IEnumerable<string> GetProjects()
        {
            IEnumerable<string> projects;
            if (!_cache.TryGetValue(GetProjectCacheKey(), out projects))
                projects = this.GenerateProjectData();

            return projects;
        }

        #endregion

        #region METHODS Private

        private IEnumerable<string> GenerateProjectData()
        {
            string[] directories = Directory.GetDirectories(Settings.ProjectsPath);
            IEnumerable<string> projects = directories.Select(r => Obfuscator.Decloak(Path.GetFileName(r)));

            // Set cache options.
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(Settings.CacheTimeout));

            _cache.Set(GetProjectCacheKey(), projects);

            return projects;
        }

        /// <summary>
        /// Generates an in-memory list of all packages in a given project. In-memory caching saves reads from the
        /// filesystem.
        /// </summary>
        /// <returns></returns>
        private IList<Package> GeneratePackageData(string project)
        {
            IList<Package> packages = new List<Package>();
            IEnumerable<string> manifestPaths = _indexReader.GetManifestPaths(project);

            foreach (string manifestPath in manifestPaths)
            {
                try
                {
                    Package package = JsonConvert.DeserializeObject<Package>(File.ReadAllText(Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment, manifestPath)));
                    packages.Add(new Package
                    {
                        CreatedUtc = package.CreatedUtc,
                        Id = package.Id,
                        Description = package.Description,
                        IsDiffed = package.IsDiffed,
                        Hash = package.Hash,
                        Tags = package.Tags
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unexpected error trying to reading manifest @ {manifestPath}");
                }
            }

            packages = packages.OrderByDescending(r => r.CreatedUtc).ToList();


            // Set cache options.
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(Settings.CacheTimeout));

            _cache.Set(GetPackageCacheKey(project), packages);

            return packages;
        }

        #endregion
    }
}
