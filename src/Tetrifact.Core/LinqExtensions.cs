using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    public static class LINQExtension
    {
        /// <summary>
        /// Returns true if all the items in in sub collection exist in main collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll1"></param>
        /// <param name="subCollection"></param>
        /// <returns></returns>
        public static bool IsSubsetOf<T>(this ICollection<T> mainCollection, ICollection<T> subCollection)
        {
            return !mainCollection.Except(subCollection).Any();
        }

    }
}
