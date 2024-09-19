using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace Tetrifact.Tests
{

    public class CacheRoot
    {
        public static Dictionary<object, TestCacheEntry> Items = new Dictionary<object, TestCacheEntry>();

    }

    public class TestCacheEntry : ICacheEntry
    {
        private object _key;
        private object _value;
        public TestCacheEntry(object key)
        { 
            _key = key;
        }
        public object Key {  get { return _key;} }

        public object Value 
        { 
            get 
            { 
                return _value; 
            } 
            set 
            {
                _value = value;
            } 
        }

        public DateTimeOffset? AbsoluteExpiration { get { return null; } set { } }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get { return null; } set { } }
        public TimeSpan? SlidingExpiration { get { return null; } set { } }

        public IList<IChangeToken> ExpirationTokens { get { return null; } }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get { return null; } }

        public CacheItemPriority Priority { get { return CacheItemPriority.Normal; } set { } }
        public long? Size { get { return null; } set { } }

        public void Dispose()
        {
            
        }
    }

    public class TetrifactTestMemoryCache : ITetrifactMemoryCache
    {
        TestMemoryCache _cache;

        public TetrifactTestMemoryCache(IMemoryCache cache)
        {
            if (!(cache is TestMemoryCache))
                throw new Exception($"{cache.GetType()} is an instance of {typeof(TestMemoryCache).Name}.");

            _cache = cache as TestMemoryCache;
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Set(string key, object item)
        {
            _cache.Set(key, item);
        }

        public void Set(string key, object item, DateTimeOffset lifetime)
        {
            _cache.Set(key, item, lifetime);
        }

        public bool TryGetValue(string key, out object item)
        {
            return _cache.TryGetValue(key, out item);
        }
    }

    /// <summary>
    /// Easy and dumb shim
    /// </summary>
    public class TestMemoryCache : IMemoryCache
    {
        public void Clear()
        {
            CacheRoot.Items.Clear();
        }

        /// <inheritdoc />
        public ICacheEntry CreateEntry(object key)
        {
            TestCacheEntry entry = new TestCacheEntry(key);
            lock (CacheRoot.Items)
            {
                if (CacheRoot.Items.ContainsKey(key))
                    CacheRoot.Items.Remove(key);

                CacheRoot.Items.Add(key, entry);
            }

            return entry;
        }

        /// <inheritdoc />
        public bool TryGetValue(object key, out object result)
        {
            lock (CacheRoot.Items)
            {
                if (CacheRoot.Items.ContainsKey(key))
                {
                    result = CacheRoot.Items[key].Value;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
        }

        /// <inheritdoc />
        public void Remove(object key)
        {
            lock (CacheRoot.Items)
                if (CacheRoot.Items.ContainsKey(key))
                    CacheRoot.Items.Remove(key);
        }


        public static void DisposeStatic()
        {
            lock (CacheRoot.Items)
                CacheRoot.Items = new Dictionary<object, TestCacheEntry>();
        }

        public void Dispose()
        {
            lock (CacheRoot.Items)
                CacheRoot.Items = new Dictionary<object, TestCacheEntry>();
        }
    }
}
