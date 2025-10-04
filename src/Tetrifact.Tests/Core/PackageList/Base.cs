using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO.Abstractions;
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
        protected ITetrifactMemoryCache TetrifactMemoryCache;

        public Base()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();

            this.MemoryCache = MemoryCacheHelper.GetInstance();
            this.TetrifactMemoryCache = TestContext.Get<ITetrifactMemoryCache>();
            this.PackageListLogger = new TestLogger<IPackageListService>();
            this.TagServiceLogger = new TestLogger<ITagsService>();
            this.PackageListCache = new Core.PackageListCache(TetrifactMemoryCache);
            this.TagService = new Core.TagsService(settings, MemoryCache, fileSystem, TagServiceLogger, PackageListCache);
            this.PackageList = new Core.PackageListService(MemoryCache, settings, new HashService(), TagService, fileSystem, this.PackageListLogger);
        }

        public void Dispose()
        {
            MemoryCache.Dispose();
        }
    }
}
