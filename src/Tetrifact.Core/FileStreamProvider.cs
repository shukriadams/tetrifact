using System.IO;


namespace Tetrifact.Core
{
    /// <summary>
    /// Provides local filesystem implementation of file stream provider.
    /// </summary>
    public class LocalFileStreamProvider : IFileStreamProvider
    {
        public Stream Read(string path) 
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
