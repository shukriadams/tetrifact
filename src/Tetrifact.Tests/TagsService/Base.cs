using Microsoft.Extensions.Caching.Memory;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.TagsService
{
    public class Base : FileSystemBase, IDisposable
    {
        protected ITagsService TagsService { get; private set; }
        protected IPackageListCache PackageListCache { get; private set; }
        protected IPackageListService PackageList { get; private set; }

        private readonly IMemoryCache _memoryCache;

        public Base()
        {
            _memoryCache = MemoryCacheHelper.GetInstance();

            this.PackageListCache = new PackageListCache(_memoryCache);
            this.TagsService = new Core.TagsService(this.Settings, this.FileSystem, new TestLogger<ITagsService>(), this.PackageListCache);
            this.PackageList = new Core.PackageListService(MemoryCacheHelper.GetInstance(), this.Settings, this.TagsService, this.FileSystem, new TestLogger<IPackageListService>());
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
