using System;
using Microsoft.Extensions.Caching.Memory;

namespace Tetrifact.Core
{
    public class TetrifactMemoryCache : ITetrifactMemoryCache
    {
        MemoryCache _cache;

        public TetrifactMemoryCache(IMemoryCache cache) 
        {
            if (!(cache is MemoryCache))
                throw new Exception($"{cache.GetType()} is an instance of {typeof(MemoryCache).Name}.");

            _cache = cache as MemoryCache;
        }

        void ITetrifactMemoryCache.Clear()
        {
            _cache.Compact(1.0);
        }

        void ITetrifactMemoryCache.Remove(string key)
        {
            _cache.Remove(key);
        }

        void ITetrifactMemoryCache.Set(string key, object item)
        {
            _cache.Set(key, item);
        }

        void ITetrifactMemoryCache.Set(string key, object item, DateTimeOffset lifetime)
        {
            _cache.Set(key, item, lifetime);
        }

        bool ITetrifactMemoryCache.TryGetValue(string key, out object item)
        {
           return _cache.TryGetValue(key, out item);
        }
    }
}
