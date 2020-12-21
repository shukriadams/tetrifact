using System.Collections.Generic;
using System.Linq;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Shim of PackageList for testing purposes, for testing integrations with IPackageList.
    /// Instead of reading packages from filesystem, returns whatever is placed in internal static collections. 
    /// These should be primed by unit tests.
    /// </summary>
    public class TestPackageList : IPackageList
    {
        public static IList<Package> Packages = new List<Package>();
        public static IList<string> PopularTags = new List<string>();
        public static Package LatestPackage;

        public static void Reset()
        {
            Packages = new List<Package>();
            PopularTags = new List<string>();
        }

        /// <summary>
        /// For testing purposes, does nothing.
        /// </summary>
        public void Clear()
        {

        }

        /// <summary>
        /// Returns a page from Packages. 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<Package> Get(int pageIndex, int pageSize)
        {
            return Packages.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public Package GetLatestWithTags(string[] tags)
        {
            return LatestPackage;
        }

        public PageableData<Package> GetPage(int pageIndex, int pageSize)
        {
            IEnumerable<Package> page = Packages.Skip(pageIndex * pageSize).Take(pageSize);
            return new PageableData<Package>(page, pageIndex, pageSize, Packages.Count);
        }

        public IEnumerable<string> GetPopularTags(int count)
        {
            return PopularTags.Take(count);   
        }

        public IEnumerable<Package> GetWithTags(string[] tags, int pageIndex, int pageSize)
        {
            return Packages.Skip(pageIndex * pageSize).Take(pageSize);
        }
    }
}
