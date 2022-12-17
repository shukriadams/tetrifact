using Ninject;
using Ninject.Parameters;
using System.Reflection;

namespace Tetrifact.Tests
{
    public class NinjectHelper
    {
        public static StandardKernel Kernel() 
        {
            StandardKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());
            return kernel;
        }

        private static T Get<T>(ConstructorArgument[] args)
        {
            StandardKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());
            return kernel.Get<T>(args);
        }

        public static T Get<T>()
        {
            return Get<T>(new ConstructorArgument[] { } );
        }

        public static T Get<T>(string name1, object arg1)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1)
            });
        }

        public static T Get<T>(string name1, object arg1, string name2, object arg2)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2)
            });
        }

        public static T Get<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3)
            });
        }

        public static T Get<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3, string name4, object arg4)
        {
            return Get<T>(new[] { 
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3),
                new ConstructorArgument(name4, arg4)
            });
        }

        public static T Get<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3, string name4, object arg4, string name5, object arg5)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3),
                new ConstructorArgument(name4, arg4),
                new ConstructorArgument(name5, arg5)
            });
        }
    }
}
