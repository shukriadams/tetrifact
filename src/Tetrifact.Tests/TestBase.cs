using Ninject;
using System.Reflection;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public abstract class TestBase
    {
        protected StandardKernel Kernel;
        protected ITetriSettings Settings;

        public TestBase()
        {
            this.Kernel = new StandardKernel();
            this.Kernel.Load(Assembly.GetExecutingAssembly());

            this.Settings = Kernel.Get<ITetriSettings>();
            
        }
    }
}
