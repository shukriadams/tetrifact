using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public interface IManagedFileSystem
    {
        string GetFileName(string name);
        string Join(string name1, string name2);
        string Join(string name1, string name2, string name3);
        string Join(string name1, string name2, string name3, string name4);
        string Join(string name1, string name2, string name3, string name4, string name5);

        void DirectoryCreate(string path);
        void DirectoryDelete(string path,bool recurse);
        void DirectoryDelete(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        IEnumerable<string> GetDirectories(string path);
        IEnumerable<string> GetFiles(string path);
        IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption);

        string ReadAllText(string path);

        Stream GetFileReadStream(string path);
        void FileDelete(string path);
    }
}
