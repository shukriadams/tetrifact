using System.IO;

namespace Tetrifact.Core
{
    public static class PathHelper
    {
        public static string DeleteFlag = "---";

        /// <summary>
        /// Returns absolute path to the latest shard for a given package. 
        /// 
        /// Throws PackageNotFoundException if requested package does not exist.
        /// </summary>
        /// <param name="indexReader"></param>
        /// <param name="project"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public static string GetLatestShardAbsolutePath(IIndexReader indexReader, string project, string package) 
        {
            ActiveTransaction currentTransactionInfo = null;

            try
            {
                currentTransactionInfo = indexReader.GetActiveTransaction(project);

                // if no transaction found, the package hasn't been written, so treat as not found
                if (currentTransactionInfo == null)
                    throw new PackageNotFoundException(package);

                string pointerPath = Path.Combine(currentTransactionInfo.Info.FullName, $"{Obfuscator.Cloak(package)}_shard");
                if (!File.Exists(pointerPath))
                    throw new PackageNotFoundException(package);

                return File.ReadAllText(pointerPath);
            }
            finally 
            {
                if (currentTransactionInfo != null)
                    currentTransactionInfo.Unlock();
            }
        }

        /// <summary>
        /// Gets path for project in data folder. Throws ProjectNotFoundException if not exists.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static string GetExpectedProjectPath(string project)
        {
            string projectPath = Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project));
            if (!Directory.Exists(projectPath))
                throw new ProjectNotFoundException(project);

            return projectPath;
        }

        public static string ResolveShardRoot(string project)
        {
            return Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ShardsFragment);
        }

        public static string ResolveTransactionRoot(string project)
        {
            return Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.TransactionsFragment);
        }

        public static string ResolveManifestsRoot(string project)
        {
            return Path.Combine(Settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment);
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
