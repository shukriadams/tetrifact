using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Tests.TagsService
{
    public class Base : TestBase, IDisposable
    {
        protected ITagsService TagsService { get; private set; }
        protected IPackageListCache PackageListCache { get; private set; }
        protected IPackageListService PackageList { get; private set; }

        private readonly IMemoryCache _memoryCache;
        protected TestLogger<ITagsService> TagsServiceLogger { get; private set; }
        protected ITetrifactMemoryCache TetrifactMemoryCache;

        public Base()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();

            _memoryCache = MemoryCacheHelper.GetInstance();
            this.TagsServiceLogger = new TestLogger<ITagsService>();
            this.TetrifactMemoryCache = TestContext.Get<ITetrifactMemoryCache>();
            this.PackageListCache = new PackageListCache(TetrifactMemoryCache);
            this.TagsService = new Core.TagsService(settings, _memoryCache, fileSystem, this.TagsServiceLogger, this.PackageListCache);
            this.PackageList = new PackageListService(MemoryCacheHelper.GetInstance(), settings, new HashService(), this.TagsService, fileSystem, new TestLogger<IPackageListService>());
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
