using Ninject;
using System.Reflection;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Generic base class for tests. Sets up minimum requirements for a test structure. 
    /// Use is optional - some tests will implement their own startup logic.
    /// </summary>
    public abstract class TestBase
    {
        protected StandardKernel Kernel;

        public TestBase()
        {
            this.Kernel = new StandardKernel();
            if (!Kernel.HasModule(typeof(Bindings).FullName))
                this.Kernel.Load(Assembly.GetExecutingAssembly());

            AppLogic appLogic = new AppLogic();
            appLogic.Start();
        }
    }
}
