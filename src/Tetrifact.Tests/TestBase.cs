using Moq;
using Ninject;
using System.Reflection;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public abstract class TestBase
    {
        protected StandardKernel Kernel;

        protected ISettings Settings;

        /// <summary>
        /// Override-friendly mock repo incase you need to instantiate concrete types then override methods on them.
        /// </summary>
        protected MockRepository MockRepository;

        public TestBase()
        {
            this.Kernel = new StandardKernel();
            this.Kernel.Load(Assembly.GetExecutingAssembly());
            this.Settings = Kernel.Get<ISettings>();
            this.MockRepository = new MockRepository(MockBehavior.Loose) { CallBase = true };
        }
    }
}
