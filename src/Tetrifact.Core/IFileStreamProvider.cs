using System.IO;

namespace Tetrifact.Core
{
    public interface IFileStreamProvider
    {
        Stream Read(string path);
    }
}
