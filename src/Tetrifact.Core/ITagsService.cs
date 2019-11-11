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
        /// <param name="project">Project name of the package to add tag to</param>
        /// <param name="package">Package identifier to add tag to</param>
        /// <param name="tag">Tag text to add</param>
        void AddTag(string project, string package, string tag);

        /// <summary>
        /// Removes a tag from a package
        /// </summary>
        /// <param name="project">Project name of the package to remove tag from</param>
        /// <param name="package">Package identifier remove tag from</param>
        /// <param name="tag">Tag text to remove</param>
        void RemoveTag(string project, string package, string tag);

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
    }
}
