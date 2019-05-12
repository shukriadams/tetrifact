using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using Tetrifact.Core;

namespace Tetrifact.Tests.PackageList
{
    public class Base : FileSystemBase, IDisposable
    {
        protected IPackageList PackageList { get; private set; }

        private IMemoryCache _memoryCache;

        public Base()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMemoryCache();
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            _memoryCache = serviceProvider.GetService<IMemoryCache>();

            this.PackageList = new Core.PackageList(_memoryCache, Settings, new TestLogger<IPackageList>());
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}
