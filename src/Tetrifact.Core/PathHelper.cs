using System.IO;

namespace Tetrifact.Core
{
    public static class PathHelper
    {
        public static string GetLatestShardPath(IndexReader indexReader, string project, string package) 
        {
            DirectoryInfo currentTransactionInfo = indexReader.GetActiveTransactionInfo(project);
            if (currentTransactionInfo == null)
                return null;

            string pointerPath = Path.Combine(currentTransactionInfo.FullName, $"{package}_shard");
            if (!File.Exists(pointerPath))
                return null;

            return File.ReadAllText(pointerPath);
        }

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

        public static string GetExpectedManifestsPath(ITetriSettings settings, string project)
        {
            string manifestsPath = Path.Combine(settings.ProjectsPath, project, Constants.ManifestsFragment);
            if (!Directory.Exists(manifestsPath))
                throw new ProjectNotFoundException(project);

            return manifestsPath;
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

        public static string ResolveTransactionRoot(ITetriSettings settings, string project)
        {
            return Path.Combine(settings.ProjectsPath, project, Constants.TransactionsFragment);
        }

        public static string ResolveManifestsRoot(ITetriSettings settings, string project)
        {
            return Path.Combine(settings.ProjectsPath, project, Constants.ManifestsFragment);
        }

        /// <summary>
        /// Converts a file or folder name to "hidden" format (~led)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetHidePath(string path)
        {
            return Path.Combine(
                Path.GetDirectoryName(path),
                $"~{Path.GetFileName(path)}"
                );
        }
    }
}
