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

        public static IEnumerable<object> CtorArgs(Type t, object overrid, bool forceMock)
        {
            return CtorArgs(t, new object[] { overrid }, forceMock);
        }

        public static IEnumerable<object> CtorArgs(Type t, object[] overrides, bool forceMock)
        {
            ConstructorInfo ctor = t.GetConstructors().FirstOrDefault();
            if (ctor == null)
                throw new Exception("no ctor found");

            StandardKernel kernel = NinjectHelper.Kernel();

            List<object> args = new List<object>();
            foreach (ParameterInfo p in ctor.GetParameters())
            {
                object arg = overrides.Where(r => r.GetType() == p.ParameterType).SingleOrDefault();
                if (arg == null)
                    arg = overrides.Where(r => p.ParameterType.IsAssignableFrom(r.GetType())).SingleOrDefault();

                try
                {
                    if (arg == null && !forceMock)
                        arg = kernel.Get(p.ParameterType);
                }
                catch (Exception ex)
                {
                    // ignore
                }

                try
                {
                    arg = CreateMock(p.ParameterType).Object;
                }
                catch (Exception ex)
                {
                    throw new Exception($"failed to create ctor arg with both ninject and moq, type {t.Name}, parameter {p.ParameterType}", ex);
                }

                args.Add(arg);
            }

            return args;
        }

        public static T WithAllMocked<T>() where T : class 
        {
            return repo.Create<T>(CtorArgs(typeof(T), new object[]{ }, true).ToArray()).Object;
        }

        public static T With<T>(Mock dependencyMock) where T : class
        {
            return repo.Create<T>(CtorArgs(typeof(T), dependencyMock.Object, false).ToArray()).Object;
        }
    }
}
