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
        public static IList<string> AllTags = new List<string>();
        public static IList<string> PackagesWithTag = new List<string>();
        public static Package LatestPackage;

        public static void Reset()
        {
            Packages = new List<Package>();
            PopularTags = new List<string>();
            AllTags = new List<string>();
            PackagesWithTag = new List<string>();
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
        public IEnumerable<Package> Get(string project, int pageIndex, int pageSize)
        {
            return Packages.Skip(pageIndex * pageSize).Take(pageSize);
        }

        public IEnumerable<string> GetAllTags(string project)
        {
            return AllTags;
        }

        public Package GetLatestWithTag(string project, string tag)
        {
            return LatestPackage;
        }

        public IEnumerable<string> GetPackagesWithTag(string project, string tag)
        {
            return PackagesWithTag;
        }

        public PageableData<Package> GetPage(string project, int pageIndex, int pageSize)
        {
            IEnumerable<Package> page = Packages.Skip(pageIndex * pageSize).Take(pageSize);
            return new PageableData<Package>(page, pageIndex, pageSize, Packages.Count);
        }

        public IEnumerable<string> GetPopularTags(string project, int count)
        {
            return PopularTags.Take(count);   
        }

        public IEnumerable<Package> GetWithTag(string project, string tag, int pageIndex, int pageSize)
        {
            return Packages.Skip(pageIndex * pageSize).Take(pageSize);
        }
    }
}
