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
        private TestContext _testContext;
        
        private MoqHelper _moq;

        private PackageHelper _packageHelper;

        protected PackageHelper PackageHelper { get { return _packageHelper; } }

        protected TestContext TestContext { get { return _testContext; } }

        protected MoqHelper MoqHelper { get { return _moq; } }

        /// <summary>
        /// This constructor acts as setup method for all tests that inherit from this type
        /// </summary>
        public TestBase()
        {
            _testContext = new TestContext();
            _moq = new MoqHelper(_testContext);
            _packageHelper = new PackageHelper(_testContext);
        }

        public void Dispose()
        {

        }
    }
}
