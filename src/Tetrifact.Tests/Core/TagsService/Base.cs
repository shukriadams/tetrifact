using System;

namespace Tetrifact.Tests.TagsService
{
    public class Base : TestBase, IDisposable
    {
        public void Dispose()
        {
            MemoryCacheHelper.GetInstance().Dispose();
        }
    }
}
