using Microsoft.Extensions.Caching.Memory;

namespace Tetrifact.DevUtils
{
    public class MemcacheShim : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            return null;
        }

        public void Dispose()
        {
            
        }

        public void Remove(object key)
        {
         
        }

        public bool TryGetValue(object key, out object value)
        {
            value = null;
            return false;
        }
    }
}
