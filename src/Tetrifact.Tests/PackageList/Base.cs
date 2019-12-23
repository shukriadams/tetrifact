using Microsoft.Extensions.Caching.Memory;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.PackageList
{
    public class Base : FileSystemBase, IDisposable
    {
        protected IPackageList PackageList { get; private set; }

        /// <summary>
        /// Several package list tests require tag operations
        /// </summary>
        protected ITagsService TagService { get; private set; }

        protected TestLogger<IPackageList> PackageListLogger { get; private set; }

        private readonly IMemoryCache _memoryCache;

        public Base()
        {
            _memoryCache = MemoryCacheHelper.GetInstance();
            this.PackageListLogger = new TestLogger<IPackageList>();
            this.PackageList = new Core.PackageList(_memoryCache, IndexReader, Settings, this.PackageListLogger);
            this.TagService = new Core.TagsService(this.Settings, this.IndexReader, this.PackageList);

        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
