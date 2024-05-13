using Moq;
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
        protected ISettings Settings;

        /// <summary>
        /// Override-friendly mock repo incase you need to instantiate concrete types then override methods on them.
        /// </summary>
        protected MockRepository MockRepository;

        /// <summary>
        /// This constructor acts as setup method for all tests that inherit from this type
        /// </summary>
        public TestBase()
        {
            TestMemoryCache.DisposeStatic();

            SettingsHelper.SetContext(this.GetType());

            this.Settings = NinjectHelper.Get<ISettings>(null);
            this.MockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };
        }

    }
}
