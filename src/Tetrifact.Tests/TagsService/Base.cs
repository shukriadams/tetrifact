using Microsoft.Extensions.Caching.Memory;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.TagsService
{
    public class Base : FileSystemBase, IDisposable
    {
        protected ITagsService TagsService { get; private set; }
        protected IPackageListCache PackageListCache { get; private set; }
        protected IPackageList PackageList { get; private set; }

        private readonly IMemoryCache _memoryCache;

        public Base()
        {
            _memoryCache = MemoryCacheHelper.GetInstance();

            this.PackageListCache = new PackageListCache(_memoryCache);
            this.TagsService = new Core.TagsService(this.Settings, new TestLogger<ITagsService>(), this.PackageListCache);
            this.PackageList = new Core.PackageList(MemoryCacheHelper.GetInstance(), this.Settings, this.TagsService, this.FileSystem, new TestLogger<IPackageList>());
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
