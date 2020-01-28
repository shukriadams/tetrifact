using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type which lists available packages on system. This type will typically use caching to improve performance, and should
    /// not be used for operations that require strict accuracy.
    /// </summary>
    public interface IPackageList
    {
        /// <summary>
        /// Gets a list of all tags in the given project
        /// </summary>
        /// <param name="project">Project name to read tags from</param>
        /// <returns>List of all tags in project</returns>
        IEnumerable<string> GetAllTags(string project);

        /// <summary>
        /// Gets a list of packages with the given tag.
        /// </summary>
        /// <param name="project">Project name to read tags from</param> 
        /// <param name="tag"></param>
        /// <returns>List of package ids containing the given tag</returns>
        IEnumerable<string> GetPackagesWithTag(string project, string tag);

        /// <summary>
        /// Flushes data for a project
        /// </summary>
        /// <param name="project"></param>
        void Clear(string project);

        /// <summary>
        /// Flushes list of projects
        /// </summary>
        public void Clear();

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
        /// Gets a list of package ids.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetPackageIds(string project, int pageIndex, int pageSize);

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

        /// <summary>
        /// Gets a list of all projects currently created on server.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetProjects();

        /// <summary>
        /// Gets all undiffed packages in a project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public IEnumerable<Package> GetUndiffedPackages(string project);
    }
}
