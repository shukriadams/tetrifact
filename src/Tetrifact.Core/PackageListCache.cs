using Microsoft.Extensions.Caching.Memory;

namespace Tetrifact.Core
{
    public class PackageListCache : IPackageListCache
    {
        private readonly IMemoryCache _cache;

        public PackageListCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Clear()
        {
            _cache.Remove(PackageListService.CacheKey);
        }
    }
}
