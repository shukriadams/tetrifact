using Ninject;
using Ninject.Parameters;
using System.Linq;
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

        private static T Get<T>(ConstructorArgument[] args, Core.ISettings settings = null)
        {
            StandardKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());
            if (settings != null){
                var binding = kernel.GetBindings(typeof(Core.ISettings)).SingleOrDefault();
                kernel.RemoveBinding(binding);
                kernel.Bind<Core.ISettings>().ToConstant(settings);
            }

            return kernel.Get<T>(args);
        }

        /// <summary>
        /// Creates an instance with entirely default bound contructor types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>(Core.ISettings settings)
        {
            return Get<T>(new ConstructorArgument[] { }, settings );
        }

        /// <summary>
        /// Creates an instance with a single constructor argument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public static T Get<T>(Core.ISettings settings, string name, object arg)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name, arg)
            }, settings);
        }

        /// <summary>
        /// Creates an instance with two constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg1Name"></param>
        /// <param name="arg1Value"></param>
        /// <param name="arg2Name"></param>
        /// <param name="arg2Value"></param>
        /// <returns></returns>
        public static T Get<T>(Core.ISettings settings, string arg1Name, object arg1Value, string arg2Name, object arg2Value)
        {
            return Get<T>(new[] {
                new ConstructorArgument(arg1Name, arg1Value),
                new ConstructorArgument(arg2Name, arg2Value)
            }, settings);
        }

        /// <summary>
        /// Creates an instance with three constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <param name="name2"></param>
        /// <param name="arg2"></param>
        /// <param name="name3"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        public static T Get<T>(Core.ISettings settings, string name1, object arg1, string name2, object arg2, string name3, object arg3)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3)
            }, settings);
        }

        /// <summary>
        /// Creates an instance with four constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <param name="name2"></param>
        /// <param name="arg2"></param>
        /// <param name="name3"></param>
        /// <param name="arg3"></param>
        /// <param name="name4"></param>
        /// <param name="arg4"></param>
        /// <returns></returns>
        public static T Get<T>(Core.ISettings settings, string name1, object arg1, string name2, object arg2, string name3, object arg3, string name4, object arg4)
        {
            return Get<T>(new[] { 
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3),
                new ConstructorArgument(name4, arg4)
            }, settings);
        }

        /// <summary>
        /// Creates an instance with five constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <param name="name2"></param>
        /// <param name="arg2"></param>
        /// <param name="name3"></param>
        /// <param name="arg3"></param>
        /// <param name="name4"></param>
        /// <param name="arg4"></param>
        /// <param name="name5"></param>
        /// <param name="arg5"></param>
        /// <returns></returns>
        public static T Get<T>(Core.ISettings settings, string name1, object arg1, string name2, object arg2, string name3, object arg3, string name4, object arg4, string name5, object arg5)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3),
                new ConstructorArgument(name4, arg4),
                new ConstructorArgument(name5, arg5)
            }, settings);
        }
    }
}
