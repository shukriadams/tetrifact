//PORTER-WRAPPER!

using System.Security.AccessControl;
using System.Windows.Markup;

namespace Tetrifact.Web.Porter_Packages {
//PORTER-WRAPPER!


using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace MadScience_SimpleDI
{
    /// <summary>
    /// A simple dependency injection system in a single file.
    /// </summary>
    public class SimpleDI
    {
        #region FIELDS
        
        private delegate object CompiledConstructor(params object[] args);

        private static RegistrationCollection ApplicationContextRegister = new RegistrationCollection();

        private static Dictionary<Type, CompiledConstructor> ApplicationContextConstructors = new Dictionary<Type, CompiledConstructor>();

        /// <summary>
        /// Registered service-implementation combinations.
        /// </summary>
        private RegistrationCollection _register = new RegistrationCollection();

        /// <summary>
        /// Caches compile constructors
        /// </summary>
        private Dictionary<Type, CompiledConstructor> _constructors = new Dictionary<Type, CompiledConstructor>();


        #endregion
        
        #region PROPERTIES 
        
        /// <summary>
        /// If true, will silently overwrite existing registrations. If false, an exception will be thrown.
        /// </summary>
        public bool OverwriteIfExists { get; set; }

        #endregion
        
        #region CTORS

        public SimpleDI()
        {
            _register = ApplicationContextRegister;
            _constructors = ApplicationContextConstructors;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="service"></param>
        /// <param name="implementation"></param>
        /// <param name="allowMultiple"></param>
        public void Register<TService, TImplementation>(string key = "", bool allowMultiple = false)
        {
            Register(typeof(TService), typeof(TImplementation), key, allowMultiple);
        }

        public void RegisterFactory<TService, TFactory>(bool isSingleton = false)
        {
            lock (_register)
            {
                Type factory = typeof(TFactory);
                Type service = typeof(TService);
                
                // simpledi factories are types, they must implement the ISimpleDIFactory interface, we use this
                // to create instances of them
                if (!typeof(ISimpleDIFactory).IsAssignableFrom(factory))
                    throw new Exception($"Factory type {factory.Name} does not implement {typeof(ISimpleDIFactory).Name}.");

                Registration registration = _register.GetService(service);

                if (!this.OverwriteIfExists && registration != null)
                    throw new Exception($"Cannot bind service type {TypeHelper.Name(service)}, a binding for this already exists ({registration}).");

                // register factory against itself, SimpleDI factories are objects, and we'll need an instance of one to provide it as a service
                if (!_register.HasService(factory))
                    _register.Add(new Registration { Service = factory, Implementation = factory });

                // register factory against service
                _register.Add(new Registration
                {
                    Service = service, 
                    Factory = factory, 
                    IsSingleton = isSingleton
                });
            }
        }

        /// <summary>
        /// Binds an implementation to a service type. Registration is required before resolving.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implementation"></param>
        /// <param name="allowMultiple"></param>
        /// <exception cref="Exception"></exception>
        public void Register(Type service, Type implementation, string key = "", bool allowMultiple = false)
        {
            lock (_register)
            {
                if (implementation.GetConstructors().Length > 1)
                    throw new Exception($"Cannot bind {TypeHelper.Name(implementation)}, type has more than one constructor.");

                if (implementation.IsAbstract)
                    throw new Exception($"Cannot bind abstract service type {TypeHelper.Name(implementation)}.");

                Registration registration = _register.FirstOrDefault(r => r.Key == key);
                if (!string.IsNullOrEmpty(key) && registration != null)
                    throw new Exception($"Cannot bind key {key}, this key already exists ({registration}).");

                bool canThrow = !this.OverwriteIfExists && !allowMultiple;
                if (canThrow && _register.HasService(service))
                    throw new Exception($"Cannot bind implementation {TypeHelper.Name(implementation)} to service {TypeHelper.Name(service)}, a binding for this service already exists.");

                _register.Add(new Registration { Service = service, Key = key, Implementation = implementation });
            }
        }

        /// <summary>
        /// Binds an instance to a service type. The given instance will always be returned for that service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="singleton"></param>
        public void RegisterSingleton<T>(object singleton)
        {
            RegisterSingleton(typeof(T), singleton);
        }

        public void RegisterFunction<T>(Func<object> callback,bool isSingleton = false)
        {
            lock (_register)
            {
                Type service = typeof(T);
                Registration registration = _register.GetService(service);

                if (!this.OverwriteIfExists && registration != null)
                    throw new Exception($"Cannot bind service type {TypeHelper.Name(service)}, a binding for this already exists ({registration}).");

                if (this.OverwriteIfExists && registration != null)
                    _register.Remove(registration);
                
                _register.Add(new Registration { Service = service, Function = callback, IsSingleton = isSingleton});
            }
        }

        public void Tag<TImplementation, TTag>()
        {
            lock (_register)
            {
                Type implementation = typeof(TImplementation);
                Registration registration = _register.GetExpectedImplementation(implementation);
                registration.ServiceTags.Add(typeof(TTag));
            }
        }

        /// <summary>
        /// Registers a generic against an implementation of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <exception cref="Exception"></exception>
        public void RegisterSingleton<T, TImplementation>()
        {
            lock (_register)
            {
                Type service = typeof(T);
                Registration registration = _register.GetService(service);

                if (!this.OverwriteIfExists && registration != null)
                    throw new Exception($"Cannot bind service type {TypeHelper.Name(service)}, a binding for this already exists ({registration}).");

                if (this.OverwriteIfExists && registration != null)
                    _register.Remove(registration);
                
                _register.Add(new Registration { Service = service, Implementation = typeof(TImplementation), IsSingleton = true });
            }
        }
        
        /// <summary>
        /// Binds an instance to a service type. The given instance will always be returned for that service.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="singleton"></param>
        /// <exception cref="Exception"></exception>
        public void RegisterSingleton(Type service, object singleton)
        {
            lock (_register)
            {
                Registration registration = _register.GetService(service);

                if (!this.OverwriteIfExists && registration != null)
                    throw new Exception($"Cannot bind service type {TypeHelper.Name(service)}, a binding for this already exists ({registration}).");

                if (this.OverwriteIfExists && registration != null)
                    _register.Remove(registration);

                _register.Add(new Registration { Service = service, Singleton = singleton });
            }
        }

        public bool IsServiceRegistered(Type service)
        {
            return _register.HasService(service);
        }

        public T ResolveByKey<T>(string key)
        {
            Type service = typeof(T);
            IEnumerable<Registration> matches = _register.Where(r => r.Key == key);
            if (!matches.Any())
                throw new Exception($"No implementations registered for key {key}.");

            return (T)ResolveInternal(matches.First(), service);
        }

        public T Resolve<T>()
        {
            Type service = typeof(T);
            IEnumerable<Registration> matches = _register.ResolveService(service);
            if (matches.Count() > 1)
                throw new Exception($"Multiple implementations are registered for service {TypeHelper.Name(service)}.");

            if (!matches.Any())
                throw new Exception($"No implementations are registered for service {TypeHelper.Name(service)}.");

            Registration registration = matches.First();
            return (T)ResolveInternal(registration, service);
        }

        /// <summary>
        /// Creates an instance of an implementation that matches the given service type. Raises exception if multiple types are registered.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object Resolve(Type service)
        {
            IEnumerable<Registration> matches = _register.ResolveService(service);
            if (matches.Count() > 1)
                throw new Exception($"Multiple implementations are registered for service {TypeHelper.Name(service)}.");

            if (!matches.Any())
                throw new Exception($"No implementations are registered for service {TypeHelper.Name(service)}.");

            return ResolveInternal(matches.First(), service);
        }
        
        /* PHASE OUT
        public object ResolveImplementation(Type implementation)
        {
            IEnumerable<Registration> matches = _register.Where(r => r.Implementation != null && TypeHelper.Name(r.Implementation, true) == TypeHelper.Name(implementation, true));
            if (matches.Count() > 1)
                throw new Exception($"Multiple implementations are registered for type {TypeHelper.Name(implementation)}.");

            if (!matches.Any())
                throw new Exception($"No implementations are registered for type {TypeHelper.Name(implementation)}.");

            return ResolveInternal(matches.First(), implementation);
        }
        */
        
        public IEnumerable<T> ResolveAll<T>()
        {
            IEnumerable<object> objects = ResolveAll(typeof(T));
            IList<T> castObjects = new List<T>();
            foreach (object o in objects)
                castObjects.Add((T)o);

            return castObjects;
        }

        /// <summary>
        /// Resolves all implementations for a given service type.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public IEnumerable<object> ResolveAll(Type service)
        {
            IList<object> instances = new List<object>();
            IEnumerable<Registration> registrations = _register.ResolveService(service);
            if (!registrations.Any())
                return instances;

            foreach (Registration registration in registrations)
                instances.Add(ResolveInternal(registration, service));

            return instances;
        }

        /// <summary>
        /// Resolves first implementation for a given service type.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public object ResolveFirst(Type service)
        {
            IEnumerable<Registration> registrations = _register.ResolveService(service);
            if (!registrations.Any())
                throw new Exception($"No implementations registered for service {TypeHelper.Name(service)}.");

            return ResolveInternal(registrations.First(), service);
        }

        /// <summary>
        /// Creates an instance of the implementation for the given registration. If implementation has sub-dependencies, creates instances
        /// of those recursively.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private object ResolveInternal(Registration registration, Type requestedService)
        {
            // it's a function, do function things, then exit
            if (registration.Function != null)
            {
                if (registration.IsSingleton && registration.Singleton == null)
                    registration.Singleton = registration.Function.Invoke(); 
                        
                if (registration.IsSingleton)
                    return registration.Singleton;
                
                return registration.Function.Invoke();
            }
            
            // it's a factory, do factory things then exit 
            if (registration.Factory != null)
            {
                ISimpleDIFactory factory = this.Resolve(registration.Factory) as ISimpleDIFactory;
                if (factory == null)
                    throw new NullReferenceException($"Factory for service {registration.Service} resolved to null.");
                
                if (registration.IsSingleton && registration.Singleton == null)
                    registration.Singleton = factory.Resolve(requestedService);
                
                if (registration.IsSingleton)
                    return registration.Singleton;

                return factory.Resolve(requestedService);
            }
            
            // type is flagged as singleton and hasn't been set, so we should set it internally 
            if (registration.IsSingleton && registration.Singleton == null)
                registration.Singleton = InstantiateFromRegistration(registration);

            // singleton instance was set, return that
            if (registration.Singleton != null)
                return registration.Singleton;
            
            // make a new instance of whatever registration contains
            return InstantiateFromRegistration(registration);
        }

        private object InstantiateFromRegistration(Registration registration)
        {
            // safety null check
            if (registration.Implementation == null)
                throw new Exception("Implementation is null ; this should not happen");

            ConstructorInfo ctor = registration.Implementation.GetConstructors().First();
            CompiledConstructor compiledConstructor = null;

            lock (_constructors)
            {
                if (!_constructors.TryGetValue(registration.Implementation, out compiledConstructor))
                {
                    compiledConstructor = BuildConstructor(ctor);
                    _constructors.Add(registration.Implementation, compiledConstructor);
                }
            }

            IList<object> args = new List<object>();

            foreach (ParameterInfo parameterInfo in ctor.GetParameters())
            {
                // inner generics parameterInfo.ParameterType.GenericTypeArguments
                if (!_register.Any(r => TypeHelper.Name(r.Service) == TypeHelper.Name(parameterInfo.ParameterType)))
                    throw new Exception($"Could not create instance of {TypeHelper.Name(registration.Implementation)}, ctor arg {TypeHelper.Name(parameterInfo.ParameterType)} is not registered");

                // recursively resolve instances for all ctor args, turtles all the way down
                object instance = Resolve(parameterInfo.ParameterType);
                args.Add(instance);
            }

            return compiledConstructor(args.ToArray());
        }

        /// <summary>
        /// Compiles a constructor based on the given constructorInfo. Compiled constructors are faster to instantation than those
        /// accessed directly with Reflection. Or at least, they were back in 2011, when this code was written.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctor"></param>
        /// <returns></returns>
        private static CompiledConstructor BuildConstructor(ConstructorInfo ctor)
        {
            ParameterInfo[] ctorParameters = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression parameters = Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp = new Expression[ctorParameters.Length];

            // pick each arg from the params array 
            // and create a typed expression of them
            for (int i = 0; i < ctorParameters.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = ctorParameters[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(parameters, index);

                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda = Expression.Lambda(typeof(CompiledConstructor), newExp, parameters);

            //compile it
            CompiledConstructor compiled = (CompiledConstructor)lambda.Compile();

            return compiled;
        }

        #endregion
    }
}


//PORTER-WRAPPER!
}
//PORTER-WRAPPER!