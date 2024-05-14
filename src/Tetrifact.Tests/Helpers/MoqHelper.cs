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
        private MockRepository repo = new MockRepository(MockBehavior.Loose) { CallBase = true };

        private TestContext _context;

        public MoqHelper(TestContext context)
        {
            _context = context;
        }

        public Mock CreateMock(Type typeToMock)
        {
            var creator = typeof(Mock<>).MakeGenericType(typeToMock);
            return (Mock)Activator.CreateInstance(creator);
        }

        public Mock<T> Mock<T>() where T : class
        {
            var creator = typeof(Mock<>).MakeGenericType(typeof(T));
            return (Mock<T>)Activator.CreateInstance(creator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="overrid"></param>
        /// <param name="forceMock">Ignores Ninject, creates empty Moq type</param>
        /// <returns></returns>
        public IEnumerable<object> ResolveConstuctorArguments(Type t, object overrid, bool forceMoq)
        {
            return ResolveConstuctorArguments(t, new object[] { overrid }, forceMoq);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="overrides">Constructor arguments to force into instance. If required arg not in this array, a default arg will be assigned.</param>
        /// <param name="forceMock">Ignores Ninject, will create empty Moq type</param>
        /// <returns></returns>
        public IEnumerable<object> ResolveConstuctorArguments(Type t, object[] overrides, bool forceMoq)
        {
            if (t.IsInterface)
                throw new Exception($"Cannot create instance of interface {t.Name} - please use a concrete type instead");

            ConstructorInfo ctor = t.GetConstructors().FirstOrDefault();
            if (ctor == null)
                throw new Exception($"No constructor found for type {t.Name}.");

            List<object> args = new List<object>();
            foreach (ParameterInfo constructorParameter in ctor.GetParameters())
            {
                // is argument matching ctor parameter in overrides array?
                object arg = overrides.Where(r => r.GetType() == constructorParameter.ParameterType).SingleOrDefault();

                // does overrides array contain a type that implements interface matching this parameter?
                if (arg == null)
                    arg = overrides.Where(r => 
                        constructorParameter.ParameterType.IsAssignableFrom(r.GetType()) 
                        || (r is Mock && constructorParameter.ParameterType.IsAssignableFrom(((Mock)r).Object.GetType()))
                    ).SingleOrDefault();

                try
                {
                    // if failed to get argument in override array, try gettin instance from ninject, and ignore errors
                    if (arg == null && !forceMoq)
                        arg = _context.Kernel.Get(constructorParameter.ParameterType);
                }
                catch (Exception)
                {
                    // ignore error, ninject doesn't need to return a type
                }

                try
                {
                    // if failed to get a ninject instance, try moq. We assume this either returns an instance or failes outright
                    if (arg == null) 
                        arg = CreateMock(constructorParameter.ParameterType).Object;
                }
                catch (Exception ex)
                {
                    throw new Exception($"failed to create ctor arg with both ninject and moq, type {t.Name}, parameter {constructorParameter.ParameterType}", ex);
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
        public T CreateInstanceWithAllMoqed<T>() where T : class 
        {
            return repo.Create<T>(ResolveConstuctorArguments(typeof(T), new object[]{ }, true).ToArray()).Object;
        }

        /// <summary>
        /// Creates an instance of type T with available Ninject IOC dependencies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencyMock"></param>
        /// <returns></returns>
        public T CreateInstanceWithSingleDependency<T>(object dependency) where T : class
        {
            return repo.Create<T>(ResolveConstuctorArguments(typeof(T), dependency, false).ToArray()).Object;
        }

        public T CreateInstanceWithDependencies<T>(object[] dependencies) where T : class
        {
            return repo.Create<T>(ResolveConstuctorArguments(typeof(T), dependencies, false).ToArray()).Object;
        }

        public Mock<T> CreateMockWithDependencies<T>(object[] dependencies) where T : class
        {
            return repo.Create<T>(ResolveConstuctorArguments(typeof(T), dependencies, false).ToArray());
        }

        /// <summary>
        /// Creates a mock of some concrete type, force casting it to an interface
        /// </summary>
        /// <typeparam name="TConcrete"></typeparam>
        /// <typeparam name="TInterfaceOut"></typeparam>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public Mock<TInterfaceOut> CreateMockWithDependencies<TConcrete, TInterfaceOut>(object[] dependencies) where TConcrete : class where TInterfaceOut : class
        {
            return repo.Create<TConcrete>(ResolveConstuctorArguments(typeof(TConcrete), dependencies, false).ToArray()).As<TInterfaceOut>();
        }
    }
}
