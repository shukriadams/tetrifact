namespace Tetrifact.Core
{
    public class PackageListCache : IPackageListCache
    {
        private readonly ITetrifactMemoryCache _cache;

        public PackageListCache(ITetrifactMemoryCache cache)
        {
            _cache = cache;
        }

        public void Clear()
        {
            // todo : no longer necessary, use clear() at call point, refactor this class out entirely
            _cache.Remove(PackageListService.CacheKey);
            _cache.Remove(TagsService.AllTagsCacheKey);
            _cache.Remove(TagsService.TagsThenPackagesKey);
            _cache.Remove(TagsService.PackagesThenTagsKey);
            _cache.Clear();
        }
    }
}
