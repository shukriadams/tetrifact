using Microsoft.Extensions.Caching.Memory;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.PackageList
{
    public class Base : FileSystemBase, IDisposable
    {
        protected IPackageList PackageList { get; private set; }
        protected IPackageListCache PackageListCache { get; private set; }
        protected new ITagsService TagService { get; private set; }
        protected TestLogger<IPackageList> PackageListLogger { get; private set; }
        protected TestLogger<ITagsService> TagServiceLogger { get; private set; }

        private readonly IMemoryCache _memoryCache;

        public Base()
        {
            _memoryCache = MemoryCacheHelper.GetInstance();
            this.PackageListLogger = new TestLogger<IPackageList>();
            this.TagServiceLogger = new TestLogger<ITagsService>();
            this.PackageListCache = new Core.PackageListCache(_memoryCache);
            this.TagService = new Core.TagsService(Settings, TagServiceLogger, PackageListCache);
            this.PackageList = new Core.PackageList(_memoryCache, Settings, TagService, this.PackageListLogger);
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
