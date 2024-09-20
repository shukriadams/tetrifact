using System;

namespace Tetrifact.Core
{
    /// <summary>
    /// Implementation of memory cache adding clear all.
    /// </summary>
    public interface ITetrifactMemoryCache
    {
        bool TryGetValue(string key, out object item);

        void Set(string key, object item);

        void Set(string key, object item, DateTimeOffset lifetime);

        void Remove(string key);

        void Clear();
    }
}
