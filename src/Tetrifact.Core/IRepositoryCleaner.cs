namespace Tetrifact.Core
{
    /// <summary>
    /// Defines a type which can remove unused files from the repository folder.
    /// </summary>
    public interface IRepositoryCleaner
    {
        /// <summary>
        /// Cleans dead files out from project folder. Dead files are manifests, shards and older transactions.
        /// </summary>
        void Clean(string project);
    }
}
