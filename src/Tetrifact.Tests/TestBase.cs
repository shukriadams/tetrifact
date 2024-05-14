using System;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Common utility base for all tests. Useful place to store instances of things which should be unique per 
    /// test run, as Xunit creates a new class instance per run. 
    /// 
    /// Utility things that are stateless should be placed in helper function.
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        /// <summary>
        /// This constructor acts as setup method for all tests that inherit from this type
        /// </summary>
        public TestBase()
        {
            SettingsHelper.SetContext(this.GetType());
        }

        public void Dispose()
        {
            TestMemoryCache.DisposeStatic();
        }
    }
}
