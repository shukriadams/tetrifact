using System;
using Microsoft.Extensions.Caching.Memory;

namespace Tetrifact.Tests.PackageList
{
    public class Base : TestBase, IDisposable
    {
        public void Dispose()
        {
            IMemoryCache memCach = this.TestContext.Get<IMemoryCache>();
            memCach.Dispose();
        }
    }
}
