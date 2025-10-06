using System;

namespace Tetrifact.Tests.PackageList
{
    public class Base : TestBase, IDisposable
    {
        public void Dispose()
        {
            MemoryCacheHelper.GetInstance().Dispose();
        }
    }
}
