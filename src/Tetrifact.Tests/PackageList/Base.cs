using Microsoft.Extensions.Caching.Memory;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.PackageList
{
    public class Base : FileSystemBase, IDisposable
    {
        protected IPackageList PackageList { get; set; }
        protected IPackageListCache PackageListCache { get; set; }
        protected new ITagsService TagService { get; set; }
        protected TestLogger<IPackageList> PackageListLogger { get; set; }
        protected TestLogger<ITagsService> TagServiceLogger { get; set; }
        protected IMemoryCache MemoryCache;

        public Base()
        {
            this.MemoryCache = MemoryCacheHelper.GetInstance();
            this.PackageListLogger = new TestLogger<IPackageList>();
            this.TagServiceLogger = new TestLogger<ITagsService>();
            this.PackageListCache = new Core.PackageListCache(MemoryCache);
            this.TagService = new Core.TagsService(Settings, TagServiceLogger, PackageListCache);
            this.PackageList = new Core.PackageList(MemoryCache, Settings, TagService, this.FileSystem, this.PackageListLogger);
        }

        public void Dispose()
        {
            MemoryCache.Dispose();
        }
    }
}
