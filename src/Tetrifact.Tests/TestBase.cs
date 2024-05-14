using Moq;
using System;
using System.IO;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Common utility base for all tests. Useful place to store instances of things which should be unique per 
    /// test run, as Xunit creates a new class instance per run. 
    /// 
    /// Utility things that are stateless should be placed in helper function.
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// This constructor acts as setup method for all tests that inherit from this type
        /// </summary>
        public TestBase()
        {
            TestMemoryCache.DisposeStatic();
          
            SettingsHelper.SetContext(this.GetType());
        }
    }
}
