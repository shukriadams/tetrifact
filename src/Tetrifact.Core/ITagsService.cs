using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type for interacting with package tags.
    /// </summary>
    public interface ITagsService
    {
        /// <summary>
        /// Adds a tag to a package
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="tag"></param>
        void AddTag(string packageId, string tag);

        /// <summary>
        /// Removes a tag from a package
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="tag"></param>
        void RemoveTag(string packageId, string tag);

        /// <summary>
        /// Gets a list of all tags.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllTags();

        /// <summary>
        /// Gets a list of packages with the given tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        IEnumerable<string> GetPackageIdsWithTag(string tag);
    }
}
