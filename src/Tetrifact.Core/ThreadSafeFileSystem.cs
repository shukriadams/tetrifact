using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public class ThreadSafeFileSystem : IManagedFileSystem
    {
        private object _readLock = new object();

        private object _changeLock = new object();

        public string GetFileName(string name)
        {
            return Path.GetFileName(name);
        }

        public string Join(string name1, string name2)
        {
            return Path.Join(name1, name2);
        }

        public string Join(string name1, string name2, string name3)
        {
            return Path.Join(name1, name2, name3);
        }

        public string Join(string name1, string name2, string name3, string name4)
        {
            return Path.Join(name1, name2, name3, name4);
        }

        public string Join(string name1, string name2, string name3, string name4, string name5)
        {
            return Path.Join(name1, name2, name3, name4, name5);
        }

        public void DirectoryCreate(string path)
        { 
            lock(_changeLock)
            {
                Directory.CreateDirectory(path);
            }
        }

        public void DirectoryDelete(string path)
        {
            lock (_changeLock)
            {
                Directory.Delete(path);
            }
        }

        public void FileDelete(string path)
        {
            lock (_changeLock)
            {
                File.Delete(path);
            }
        }

        public void DirectoryDelete(string path, bool recurse)
        {
            lock (_changeLock)
            {
                Directory.Delete(path, recurse);
            }
        }

        public bool DirectoryExists(string path)
        {
            lock (_readLock)
            {
                return Directory.Exists(path);
            }
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            lock (_readLock)
            {
                return Directory.GetDirectories(path);
            }
        }

        public IEnumerable<string> GetFiles(string path)
        {
            lock (_readLock)
            {
                return Directory.GetFiles(path);
            }
        }

        public IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            lock (_readLock)
            {
                return Directory.GetFiles(path, searchPattern, searchOption);
            }
        }

        public bool FileExists(string path)
        {
            lock (_readLock)
            {
                return File.Exists(path);
            }
        }

        public string ReadAllText(string path)
        {
            lock (_readLock)
            {
                return File.ReadAllText(path);
            }
        }

        public Stream GetFileReadStream(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
