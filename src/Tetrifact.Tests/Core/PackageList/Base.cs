using Microsoft.Extensions.Caching.Memory;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.PackageList
{
    public class Base : FileSystemBase, IDisposable
    {
        protected IPackageListService PackageList { get; set; }
        protected IPackageListCache PackageListCache { get; set; }
        protected new ITagsService TagService { get; set; }
        protected TestLogger<IPackageListService> PackageListLogger { get; set; }
        protected TestLogger<ITagsService> TagServiceLogger { get; set; }
        protected IMemoryCache MemoryCache;

        public Base()
        {
            this.MemoryCache = MemoryCacheHelper.GetInstance();
            this.PackageListLogger = new TestLogger<IPackageListService>();
            this.TagServiceLogger = new TestLogger<ITagsService>();
            this.PackageListCache = new Core.PackageListCache(MemoryCache);
            this.TagService = new Core.TagsService(Settings, this.FileSystem, TagServiceLogger, PackageListCache);
            this.PackageList = new Core.PackageListService(MemoryCache, Settings, new HashService(), TagService, this.FileSystem, this.PackageListLogger);
        }

        public void Dispose()
        {
            MemoryCache.Dispose();
        }
    }
}
