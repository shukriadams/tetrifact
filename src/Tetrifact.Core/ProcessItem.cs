using System;

namespace Tetrifact.Core
{
    public class ProcessItem
    {
        /// <summary>
        /// Unique id of lock
        /// </summary>
        public string Id { get;set; }

        /// <summary>
        /// Grouping name for item. 
        /// </summary>
        public ProcessCategories Category { get; set; }

        /// <summary>
        /// If set, the time lock item was created. If not set, lock never times out
        /// </summary>
        public DateTime? AddedUTC { get; set; }

        /// <summary>
        /// If set, the max age lock item can have before being autopurged.
        /// </summary>
        public TimeSpan? MaxLifespan { get; set; }

        /// <summary>
        /// Copies an instance with no object references to source.
        /// </summary>
        /// <returns></returns>
        public ProcessItem Clone() 
        { 
            return this.MemberwiseClone() as ProcessItem;
        }
    
        /// <summary>
        /// Returns a string presentation of instance, for logging purposes.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{{{this.GetType().Name} Id:{Id}, Category:{Category}, AddedUtc:{AddedUTC}, MaxLifespace:{MaxLifespan} }}";
        }
    }
}
