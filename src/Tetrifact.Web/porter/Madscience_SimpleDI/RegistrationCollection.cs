

namespace Tetrifact.Web.Porter_Packages 
{
    
    
using System;
using System.Collections.Generic;
using System.Linq;
    
namespace MadScience_SimpleDI
{
    class RegistrationCollection : List<Registration>
    {
        public bool HasService(Type type)
        {
            return this.Any(registration => TypeHelper.Name(registration.Service) == TypeHelper.Name(type) );
        }

        public bool HasTag(Type type)
        {
            return this.Any(registration => registration.ServiceTags.Any(serviceType => TypeHelper.Name(serviceType) == TypeHelper.Name(type))  );
        }

        public bool HasImplementation(Type type)
        {
            return this.Any(registration => TypeHelper.Name(registration.Implementation) == TypeHelper.Name(type) );
        }

        public Registration GetImplementation(Type implementation)
        {
            return GetImplementation(implementation, false);
        }
        
        public Registration GetExpectedImplementation(Type implementation)
        {
            return GetImplementation(implementation, true);
        }

        private Registration GetImplementation(Type implementation, bool throwIfNotFound)
        {
            Registration registration = this.Where(r => r.Implementation != null && TypeHelper.Name(r.Implementation) == TypeHelper.Name(implementation)).FirstOrDefault();
            if (registration == null && throwIfNotFound)
                throw new Exception($"Could not find a service that implements expected type  {TypeHelper.Name(implementation)}.");
            return registration;
        }

        public Registration GetExpectedService(Type service)
        {
            return GetService(service, true);
        }

        public Registration GetService(Type service)
        {
            return GetService(service, false);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Registration GetService(Type service, bool throwIfNotFound)
        {
            Registration registration = this.Where(r => r.Service != null && TypeHelper.Name(r.Service) == TypeHelper.Name(service)).FirstOrDefault();
            if (registration == null && throwIfNotFound)
                throw new Exception($"No registered service {TypeHelper.Name(service)}.");
            return registration;
        }

        /// <summary>
        /// Gets
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public IEnumerable<Registration> ResolveService(Type service)
        {
            return this.Where(registration => 
                TypeHelper.Name(registration.Service) == TypeHelper.Name(service) 
                || registration.ServiceTags.Any(serviceType => TypeHelper.Name(serviceType) == TypeHelper.Name(service)));
        }
    }
}

}