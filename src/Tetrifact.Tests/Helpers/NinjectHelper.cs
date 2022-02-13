using Ninject;
using Ninject.Parameters;
using System.Reflection;

namespace Tetrifact.Tests
{
    public class NinjectHelper
    {
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
            return Get<T>(Ctor(name1, arg1));
        }

        public static T Get<T>(string name1, object arg1, string name2, object arg2)
        {
            return Get<T>(Ctor(name1, arg1, name2, arg2));
        }

        public static T Get<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3)
        {
            return Get<T>(Ctor(name1, arg1, name2, arg2, name3, arg3));
        }

        public static ConstructorArgument[] Ctor(string name1, object arg1)
        {
            return new[] { new ConstructorArgument(name1, arg1)};
        }

        public static ConstructorArgument[] Ctor(string name1, object arg1, string name2, object arg2)
        {
            return new[] { new ConstructorArgument(name1, arg1), new ConstructorArgument(name2, arg2) };
        }

        public static ConstructorArgument[] Ctor(string name1, object arg1, string name2, object arg2, string name3, object arg3)
        {
            return new[] { new ConstructorArgument(name1, arg1), new ConstructorArgument(name2, arg2), new ConstructorArgument(name3, arg3) };
        }
    }
}
