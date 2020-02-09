namespace Tetrifact.Core
{
    public interface IProjectService
    {
        /// <summary>
        /// Creates a project.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        ProjectCreateResult CreateProject(string name);
    }
}
