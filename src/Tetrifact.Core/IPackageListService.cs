using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type which lists available packages on system. Listing includes paging logic. Note that there is some overlap between this
    /// and ITagServices, this should be refactored out, ITagServices' has no paging logic though.
    /// </summary>
    public interface IPackageListService
    {
        /// <summary>
        /// Gets a list of size count of the most popular tags
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<string> GetPopularTags(int count);

        /// <summary>
        /// Gets the latest package with the given tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Package GetLatestWithTags(string[] tags);

        /// <summary>
        /// Gets a page of packages with the given tags.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IEnumerable<Package> GetWithTags(string[] tags, int pageIndex, int pageSize);

        /// <summary>
        /// Gets a page of packages.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IEnumerable<Package> Get(int pageIndex, int pageSize);

        /// <summary>
        /// Gets a pageable collection of packages.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PageableData<Package> GetPage(int pageIndex, int pageSize);

        /// <summary>
        /// Searches packages by id and tag
        /// </summary>
        /// <param name="searchtext"></param>
        /// <returns></returns>
        PageableData<Package> Find(string searchtext, int pageIndex, int pageSize);
    }
}
