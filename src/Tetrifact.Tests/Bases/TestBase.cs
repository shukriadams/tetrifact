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
        protected ISettings Settings;

        public TestBase()
        {
            this.Kernel = new StandardKernel();
            this.Kernel.Load(Assembly.GetExecutingAssembly());
            this.Settings = Kernel.Get<ISettings>();
            AppLogic appLogic = new AppLogic(this.Settings);
            appLogic.Start();
        }
    }
}
