

namespace Tetrifact.Web.Porter_Packages 
{

    
using System;
using System.Collections.Generic;
    
namespace MadScience_SimpleDI
{
    public class Registration
    {
        private Type _service;
        
        private Type _implementation;
        
        /// <summary>
        /// Plugins can be registered by unique strings.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Service or interface types an implementation is registered by.
        /// </summary>
        public Type Service 
        { 
            get
            {
                return _service;
            }
            set
            {
                _service = value;
                if (value != null)
                    this.ServiceTags.Add(value);
            }
        }


        /// <summary>
        /// Service or interface types an implementation is registered by.
        /// </summary>
        public IList<Type> ServiceTags { get; } = new List<Type>();

        /// <summary>
        /// The concrete type that fulfills the service requirement. Replaced by Factory or Singleton.
        /// </summary>
        public Type Implementation 
        { 
            get
            {
                return _implementation;
            }
            set
            {
                _implementation = value;
                if (value != null)
                    this.ServiceTags.Add(value);
            }
        }

        /// <summary>
        /// A factory that provides an implementation. Replaced by Implementation or Singleton.
        /// </summary>
        public Type Factory { get; set; }

        public Func<object> Function { get; set; }
        
        /// <summary>
        /// A global instance that fulfills service. Replaced by Implementation or Factory.
        /// </summary>
        public object Singleton { get; set; }
        
        /// <summary>
        /// If true, will return this.Singleton on request. If Singleton is null and Implementation is set, will
        /// instantiate this.Singleton first.  
        /// </summary>
        public bool IsSingleton { get; set; }

        public override string ToString()
        {
            string description = "";
                
            if (string.IsNullOrEmpty(this.Key))
                description += "Key not set; ";
            else
                description += $"Key {this.Key}; ";

            if (this.Service != null)
                description += $"Service {this.Service}; ";
                    
            description += $"Service tags {string.Join(",", this.ServiceTags)}; ";
            
            if (this.Implementation != null)
                description += $"Implementation {this.Implementation.FullName}; ";
            else 
            {
                if (this.Factory != null)
                    description += $"Factory {this.Factory.FullName}; ";
                else
                    description += "Registration invalid";
            }

            return description;
        }
    }
}


}