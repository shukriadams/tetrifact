using Microsoft.Extensions.Caching.Memory;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.PackageList
{
    public class Base : FileSystemBase, IDisposable
    {
        protected IPackageList PackageList { get; private set; }

        protected TestLogger<IPackageList> PackageListLogger { get; private set; }

        private readonly IMemoryCache _memoryCache;

        public Base()
        {
            _memoryCache = MemoryCacheHelper.GetInstance();
            this.PackageListLogger = new TestLogger<IPackageList>();
            this.PackageList = new Core.PackageList(_memoryCache, Settings, this.PackageListLogger);
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
