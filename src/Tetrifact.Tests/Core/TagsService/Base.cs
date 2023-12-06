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
        protected TestLogger<ITagsService> TagsServiceLogger { get; private set; }

        public Base()
        {
            _memoryCache = MemoryCacheHelper.GetInstance();
            this.TagsServiceLogger = new TestLogger<ITagsService>();
            this.PackageListCache = new PackageListCache(_memoryCache);
            this.TagsService = new Core.TagsService(this.Settings, this.FileSystem, this.TagsServiceLogger, this.PackageListCache);
            this.PackageList = new PackageListService(MemoryCacheHelper.GetInstance(), this.Settings, new HashService(), this.TagsService, this.FileSystem, new TestLogger<IPackageListService>());
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
