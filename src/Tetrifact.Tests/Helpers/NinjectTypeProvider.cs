using Tetrifact.Core;
using Ninject;
using System.Reflection;

namespace Tetrifact.Tests
{
    public class NinjectTypeProvider : ITypeProvider
    {
        StandardKernel _kernel;

        public NinjectTypeProvider() 
        {
            _kernel = new StandardKernel();
        }

        public T GetInstance<T>()
        {
            if (!_kernel.HasModule(typeof(Bindings).FullName))
                _kernel.Load(Assembly.GetExecutingAssembly());

            return _kernel.Get<T>();
        }
    }
}
