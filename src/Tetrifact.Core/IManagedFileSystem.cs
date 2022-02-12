using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public interface IManagedFileSystem
    {
        DirectoryInfo GetDirectory(string path);
        void FileMove(string source, string destination);

        void DirectoryCreate(string path);
        void DirectoryDelete(string path,bool recurse);
        void DirectoryDelete(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        IEnumerable<string> GetDirectories(string path);
        IEnumerable<string> GetFiles(string path);
        IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption);

        string ReadAllText(string path);
        void WriteAllText(string path, string text);
        Stream GetFileReadStream(string path);
        void FileDelete(string path);
    }
}
