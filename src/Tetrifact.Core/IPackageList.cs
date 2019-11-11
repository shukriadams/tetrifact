using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type which lists available packages on system. Listing includes paging logic. Note that there is some overlap between this
    /// and ITagServices, this should be refactored out, ITagServices' has no paging logic though.
    /// </summary>
    public interface IPackageList
    {
        void Clear();

        /// <summary>
        /// Gets a list of size count of the most popular tags
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<string> GetPopularTags(string project, int count);

        /// <summary>
        /// Gets the latest package with the given tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Package GetLatestWithTag(string project, string tag);

        /// <summary>
        /// Gets a page of packages with the given tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IEnumerable<Package> GetWithTag(string project, string tag, int pageIndex, int pageSize);

        /// <summary>
        /// Gets a page of packages.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IEnumerable<Package> Get(string project, int pageIndex, int pageSize);

        /// <summary>
        /// Gets a pageable collection of packages.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PageableData<Package> GetPage(string project, int pageIndex, int pageSize);
    }
}
