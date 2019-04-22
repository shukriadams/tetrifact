using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type which lists available packages on system. Listing includes paging logic. 
    /// </summary>
    public interface IPackageList
    {
        void Clear();

        IEnumerable<string> GetPopularTags(int count);

        IEnumerable<Package> GetWithTag(string tag, int pageIndex, int pageSize);

        IEnumerable<Package> Get(int pageIndex, int pageSize);

        PageableData<Package> GetPage(int pageIndex, int pageSize);
    }
}
