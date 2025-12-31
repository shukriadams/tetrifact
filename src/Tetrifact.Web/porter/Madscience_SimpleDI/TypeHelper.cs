namespace Tetrifact.Web.Porter_Packages {
//PORTER-WRAPPER!

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace MadScience_SimpleDI
{
    /// <summary>
    /// Utilities for querying types.
    /// </summary>
    public class TypeHelper
    {
        static Assembly _commonAssembly;

        static TypeHelper()
        {
            _commonAssembly = typeof(TypeHelper).Assembly;
        }

        public static string Name<T>()
        {
            return Name(typeof(T));
        }

        public static string Name(object obj)
        {
            return Name(obj.GetType());
        }

        public static string Name(Type type)
        {
            string name = $"{type.Namespace}.{type.Name}";
            if (type.IsGenericType && type.GenericTypeArguments.Any())
            {
                if (name.EndsWith("`1"))
                    name = name.Substring(0, name.Length - 2);

                name += string.Join("_<>_", type.GenericTypeArguments.ToList());
            }
                    
            //if (removeGeneric && name.EndsWith("`1"))
            //    name = name.Substring(0, name.Length - 2);

            return name;
        }

        public static Assembly GetAssembly(string namespc)
        {
            Assembly assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetName().Name == namespc)
                .FirstOrDefault();

            if (assembly == null)
                assembly = Assembly.Load(namespc);

            return assembly;
        }

        /// <summary>
        /// Gets first occurrence of the given attribute on the source type.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(Type source)
        {
            return TypeDescriptor.GetAttributes(source).OfType<T>().FirstOrDefault();
        }

        public static Type? ResolveType(string namespacedType)
        {
            // TODO - cache type lookup for performance
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? concreteType = a.GetType(namespacedType);
                if (concreteType != null)
                    return concreteType;
            }

            // couldn't resolve type, does it live in an assembly that needs to be loaded?

            return null;
        }

        public static Type GetCommonType(string typeNamespacedName)
        {
            return _commonAssembly.GetType(typeNamespacedName);
        }
    }
}

}