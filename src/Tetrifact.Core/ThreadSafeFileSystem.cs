using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public class ThreadSafeFileSystem : IManagedFileSystem
    {
        private object _readLock = new object();

        private object _changeLock = new object();

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

        public Stream GetFileStream()
        { 

        }
    }
}
