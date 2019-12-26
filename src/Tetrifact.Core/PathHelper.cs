using System.IO;

namespace Tetrifact.Core
{
    public static class PathHelper
    {
        public static string DeleteFlag = "---";

        public static string GetLatestShardPath(IndexReader indexReader, string project, string package) 
        {
            DirectoryInfo currentTransactionInfo = indexReader.GetActiveTransactionInfo(project);
            if (currentTransactionInfo == null)
                return null;

            string pointerPath = Path.Combine(currentTransactionInfo.FullName, $"{Obfuscator.Cloak(package)}_shard");
            if (!File.Exists(pointerPath))
                return null;

            return File.ReadAllText(pointerPath);
        }

        /// <summary>
        /// Gets path for project in data folder. Throws ProjectNotFoundException if not exists.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static string GetExpectedProjectPath(ISettings settings, string project)
        {
            string projectPath = Path.Combine(settings.ProjectsPath, Obfuscator.Cloak(project));
            if (!Directory.Exists(projectPath))
                throw new ProjectNotFoundException(project);

            return projectPath;
        }

        public static string GetExpectedManifestsPath(ISettings settings, string project)
        {
            string manifestsPath = Path.Combine(settings.ProjectsPath, project, Constants.ManifestsFragment);
            if (!Directory.Exists(manifestsPath))
                throw new ProjectNotFoundException(project);

            return manifestsPath;
        }

        public static string ResolveFinalFileBinPath(ISettings settings, string project, string package, string filePath) 
        {
            return Path.Combine(settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ShardsFragment, Obfuscator.Cloak(package), filePath, "bin");
        }

        public static string ResolveFinalFilePathPath(ISettings settings, string project, string package, string filePath)
        {
            return Path.Combine(settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ShardsFragment, Obfuscator.Cloak(package), filePath, "patch");
        }

        public static string ResolveShardRoot(ISettings settings, string project)
        {
            return Path.Combine(settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ShardsFragment);
        }

        public static string ResolveTransactionRoot(ISettings settings, string project)
        {
            return Path.Combine(settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment);
        }

        public static string ResolveManifestsRoot(ISettings settings, string project)
        {
            return Path.Combine(settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment);
        }

        /// <summary>
        /// Converts a file or folder name to "deleting" format 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDeletingPath(string path)
        {
            return Path.Combine(
                Path.GetDirectoryName(path),
                $"{PathHelper.DeleteFlag}{Path.GetFileName(path)}"
                );
        }
    }
}
