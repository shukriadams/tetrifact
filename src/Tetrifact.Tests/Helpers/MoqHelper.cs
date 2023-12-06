using Moq;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tetrifact.Tests
{
    public class MoqHelper
    {
        private static MockRepository repo = new MockRepository(MockBehavior.Loose) { CallBase = true };

        public static Mock CreateMock(Type typeToMock)
        {
            var creator = typeof(Mock<>).MakeGenericType(typeToMock);
            return (Mock)Activator.CreateInstance(creator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="overrid"></param>
        /// <param name="forceMock">Ignores Ninject, creates empty Moq type</param>
        /// <returns></returns>
        public static IEnumerable<object> CtorArgs(Type t, object overrid, bool forceMoq)
        {
            return CtorArgs(t, new object[] { overrid }, forceMoq);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="overrides"></param>
        /// <param name="forceMock">Ignores Ninject, will create empty Moq type</param>
        /// <returns></returns>
        public static IEnumerable<object> CtorArgs(Type t, object[] overrides, bool forceMoq)
        {
            if (t.IsInterface)
                throw new Exception($"Cannot create instance of interface {t.Name} - please use a class instead");

            ConstructorInfo ctor = t.GetConstructors().FirstOrDefault();
            if (ctor == null)
                throw new Exception("no ctor found");

            StandardKernel kernel = NinjectHelper.Kernel();

            List<object> args = new List<object>();
            foreach (ParameterInfo p in ctor.GetParameters())
            {
                object arg = overrides.Where(r => r.GetType() == p.ParameterType).SingleOrDefault();
                if (arg == null)
                    arg = overrides.Where(r => 
                        p.ParameterType.IsAssignableFrom(r.GetType()) 
                        || (r is Mock && p.ParameterType.IsAssignableFrom(((Mock)r).Object.GetType()))
                    ).SingleOrDefault();

                try
                {
                    if (arg == null && !forceMoq)
                        arg = kernel.Get(p.ParameterType);
                }
                catch (Exception ex)
                {
                    // ignore
                }

                try
                {
                    if (arg == null) 
                        arg = CreateMock(p.ParameterType).Object;
                }
                catch (Exception ex)
                {
                    throw new Exception($"failed to create ctor arg with both ninject and moq, type {t.Name}, parameter {p.ParameterType}", ex);
                }

                if (arg is Mock)
                { 
                    args.Add(((Mock)arg).Object);
                }
                else
                    args.Add(arg);
            }

            return args;
        }

        /// <summary>
        /// Creates an instance of type T with all IOC dependencies mocked.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateInstanceWithAllMoqed<T>() where T : class 
        {
            return repo.Create<T>(CtorArgs(typeof(T), new object[]{ }, true).ToArray()).Object;
        }

        /// <summary>
        /// Creates an instance of type T with available Ninject IOC dependencies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyMock"></param>
        /// <returns></returns>
        public static T CreateInstanceWithSingleDependency<T>(object dependency) where T : class
        {
            return repo.Create<T>(CtorArgs(typeof(T), dependency, false).ToArray()).Object;
        }

        public static T CreateInstanceWithDependencies<T>(object[] dependencies) where T : class
        {
            return repo.Create<T>(CtorArgs(typeof(T), dependencies, false).ToArray()).Object;
        }
    }
}
