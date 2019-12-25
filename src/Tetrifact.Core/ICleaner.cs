namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type which can remove unused files from the repository folder.
    /// </summary>
    public interface ICleaner
    {
        /// <summary>
        /// Cleans dead files and folders from project folder. Dead files are manifests, shards and older transactions.
        /// </summary>
        void Clean(string project);
    }
}
