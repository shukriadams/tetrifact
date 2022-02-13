using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace Tetrifact.Tests
{

    public class TestCacheEntry : ICacheEntry
    {
        public object Key {  get { return "";} }

        public object Value { get { return ""; } set { } }

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
    /// <summary>
    /// Easy and dumb shim
    /// </summary>
    public class TestMemoryCache : IMemoryCache
    {
        /// <inheritdoc />
        public ICacheEntry CreateEntry(object key)
        {
            return new TestCacheEntry();
        }

        /// <inheritdoc />
        public bool TryGetValue(object key, out object result)
        {
            result = null;
            return false;
        }

        /// <inheritdoc />
        public void Remove(object key)
        {
            
        }
       
        public void Dispose()
        {

        }
    }
}
