using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface IManagedFileSystem
    {
        void DirectoryCreate(string path);
        void DirectoryDelete(string path,bool recurse);
        void DirectoryDelete(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        IEnumerable<string> GetDirectories(string path);
        string ReadAllText(string path);
    }
}
