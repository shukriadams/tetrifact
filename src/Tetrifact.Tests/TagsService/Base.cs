using Microsoft.Extensions.Caching.Memory;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.TagsService
{
    public class Base : FileSystemBase, IDisposable
    {
        protected ITagsService TagsService { get; private set; }

        protected IPackageList PackageList { get; private set; }

        private readonly IMemoryCache _memoryCache;

        public Base()
        {
            _memoryCache = MemoryCacheHelper.GetInstance();

            this.PackageList = new Core.PackageList(_memoryCache, this.Settings, new TestLogger<IPackageList>());
            this.TagsService = new Core.TagsService(this.Settings, new TestLogger<ITagsService>(), this.PackageList);
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
