using System.IO;

namespace Tetrifact.Core
{
    public static class PathHelper
    {
        /// <summary>
        /// Gets path for project in data folder. Throws ProjectNotFoundException if not exists.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static string GetExpectedProjectPath(ITetriSettings settings, string project)
        {
            string projectPath = Path.Combine(settings.ProjectsPath, project);
            if (!Directory.Exists(projectPath))
                throw new ProjectNotFoundException(project);

            return projectPath;
        }

        public static string GetExpectedRepositoryPath(ITetriSettings settings, string project)
        {
            string reposPath = Path.Combine(settings.ProjectsPath, project, Constants.RepositoryFragment);
            if (!Directory.Exists(reposPath))
                throw new MissingProjectStructure(project);

            return reposPath;
        }

        public static string GetExpectedPackagesPath(ITetriSettings settings, string project)
        {
            string packagesPath = Path.Combine(settings.ProjectsPath, project, Constants.PackagesFragment);
            if (!Directory.Exists(packagesPath))
                throw new ProjectNotFoundException(project);

            return packagesPath;
        }

        public static string GetExpectedTagsPath(ITetriSettings settings, string project)
        {
            string tagsPath = Path.Combine(settings.ProjectsPath, project, Constants.TagsFragment);
            if (!Directory.Exists(tagsPath))
                throw new ProjectNotFoundException(project);

            return tagsPath;
        }

        public static string ResolveFinalFileBinPath(ITetriSettings settings, string project, string package, string filePath) 
        {
            return Path.Combine(settings.ProjectsPath, project, Constants.ShardsFragment, package, filePath, "bin");
        }

        public static string ResolveFinalFilePathPath(ITetriSettings settings, string project, string package, string filePath)
        {
            return Path.Combine(settings.ProjectsPath, project, Constants.ShardsFragment, package, filePath, "patch");
        }

        public static string ResolveShardRoot(ITetriSettings settings, string project)
        {
            return Path.Combine(settings.ProjectsPath, project, Constants.ShardsFragment);
        }

        /// <summary>
        /// Note : doesn't check if path exists.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public static string GetExpectedHeadDirectoryPath(ITetriSettings settings, string project) 
        {
            string headPath = Path.Combine(settings.ProjectsPath, project, Constants.HeadFragment);
            if (!Directory.Exists(headPath))
                throw new ProjectNotFoundException(project);

            return headPath;
        }
    }
}
