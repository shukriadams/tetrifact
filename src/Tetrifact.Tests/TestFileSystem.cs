using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tetrifact.Tests
{
    public class TestFile : IFile
    {
        IFileSystem IFile.FileSystem => throw new NotImplementedException();

        void IFile.AppendAllLines(string path, IEnumerable<string> contents)
        {
            throw new NotImplementedException();
        }

        void IFile.AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        Task IFile.AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IFile.AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IFile.AppendAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }

        void IFile.AppendAllText(string path, string contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        Task IFile.AppendAllTextAsync(string path, string contents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IFile.AppendAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        StreamWriter IFile.AppendText(string path)
        {
            throw new NotImplementedException();
        }

        void IFile.Copy(string sourceFileName, string destFileName)
        {
            throw new NotImplementedException();
        }

        void IFile.Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }

        Stream IFile.Create(string path)
        {
            throw new NotImplementedException();
        }

        Stream IFile.Create(string path, int bufferSize)
        {
            throw new NotImplementedException();
        }

        Stream IFile.Create(string path, int bufferSize, FileOptions options)
        {
            throw new NotImplementedException();
        }

        StreamWriter IFile.CreateText(string path)
        {
            throw new NotImplementedException();
        }

        void IFile.Decrypt(string path)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(string path)
        {
            throw new NotImplementedException();
        }

        void IFile.Encrypt(string path)
        {
            throw new NotImplementedException();
        }

        bool IFile.Exists(string path)
        {
            throw new NotImplementedException();
        }

        FileSecurity IFile.GetAccessControl(string path)
        {
            throw new NotImplementedException();
        }

        FileSecurity IFile.GetAccessControl(string path, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        FileAttributes IFile.GetAttributes(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IFile.GetCreationTime(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IFile.GetCreationTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IFile.GetLastAccessTime(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IFile.GetLastAccessTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IFile.GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IFile.GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        void IFile.Move(string sourceFileName, string destFileName)
        {
            throw new NotImplementedException();
        }

        Stream IFile.Open(string path, FileMode mode)
        {
            throw new NotImplementedException();
        }

        Stream IFile.Open(string path, FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        Stream IFile.Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        Stream IFile.OpenRead(string path)
        {
            throw new NotImplementedException();
        }

        StreamReader IFile.OpenText(string path)
        {
            throw new NotImplementedException();
        }

        Stream IFile.OpenWrite(string path)
        {
            throw new NotImplementedException();
        }

        byte[] IFile.ReadAllBytes(string path)
        {
            throw new NotImplementedException();
        }

        Task<byte[]> IFile.ReadAllBytesAsync(string path, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        string[] IFile.ReadAllLines(string path)
        {
            throw new NotImplementedException();
        }

        string[] IFile.ReadAllLines(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        Task<string[]> IFile.ReadAllLinesAsync(string path, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string[]> IFile.ReadAllLinesAsync(string path, Encoding encoding, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        string IFile.ReadAllText(string path)
        {
            throw new NotImplementedException();
        }

        string IFile.ReadAllText(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        Task<string> IFile.ReadAllTextAsync(string path, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<string> IFile.ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IFile.ReadLines(string path)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IFile.ReadLines(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        void IFile.Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
        {
            throw new NotImplementedException();
        }

        void IFile.Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        void IFile.SetAccessControl(string path, FileSecurity fileSecurity)
        {
            throw new NotImplementedException();
        }

        void IFile.SetAttributes(string path, FileAttributes fileAttributes)
        {
            throw new NotImplementedException();
        }

        void IFile.SetCreationTime(string path, DateTime creationTime)
        {
            throw new NotImplementedException();
        }

        void IFile.SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            throw new NotImplementedException();
        }

        void IFile.SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        void IFile.SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        void IFile.SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        void IFile.SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }

        void IFile.WriteAllBytes(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        Task IFile.WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IFile.WriteAllLines(string path, IEnumerable<string> contents)
        {
            throw new NotImplementedException();
        }

        void IFile.WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        void IFile.WriteAllLines(string path, string[] contents)
        {
            throw new NotImplementedException();
        }

        void IFile.WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        Task IFile.WriteAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IFile.WriteAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IFile.WriteAllLinesAsync(string path, string[] contents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IFile.WriteAllLinesAsync(string path, string[] contents, Encoding encoding, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void IFile.WriteAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }

        void IFile.WriteAllText(string path, string contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        Task IFile.WriteAllTextAsync(string path, string contents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IFile.WriteAllTextAsync(string path, string contents, Encoding encoding, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class TestDirectory : IDirectory
    {
        IFileSystem IDirectory.FileSystem => throw new NotImplementedException();

        IDirectoryInfo IDirectory.CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        IDirectoryInfo IDirectory.CreateDirectory(string path, DirectorySecurity directorySecurity)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(string path)
        {
            Directory.Delete(path);
        }

        public virtual void Delete(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }

        IEnumerable<string> IDirectory.EnumerateDirectories(string path)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateDirectories(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateFiles(string path)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateFiles(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateFileSystemEntries(string path)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateFileSystemEntries(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        IEnumerable<string> IDirectory.EnumerateFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            throw new NotImplementedException();
        }

        bool IDirectory.Exists(string path)
        {
            return Directory.Exists(path);
        }

        DirectorySecurity IDirectory.GetAccessControl(string path)
        {
            throw new NotImplementedException();
        }

        DirectorySecurity IDirectory.GetAccessControl(string path, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        DateTime IDirectory.GetCreationTime(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IDirectory.GetCreationTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        string IDirectory.GetCurrentDirectory()
        {
            throw new NotImplementedException();
        }

        public virtual string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public virtual string[] GetDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern);
        }

        public virtual string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetDirectories(path, searchPattern, searchOption);
        }

        public virtual string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return Directory.GetDirectories(path, searchPattern, enumerationOptions);
        }

        string IDirectory.GetDirectoryRoot(string path)
        {
            throw new NotImplementedException();
        }

        public virtual string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public virtual string[] GetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }

        public virtual string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }

        public virtual string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions)
        {
            return Directory.GetFiles(path, searchPattern, enumerationOptions);
        }

        string[] IDirectory.GetFileSystemEntries(string path)
        {
            throw new NotImplementedException();
        }

        string[] IDirectory.GetFileSystemEntries(string path, string searchPattern)
        {
            throw new NotImplementedException();
        }

        DateTime IDirectory.GetLastAccessTime(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IDirectory.GetLastAccessTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IDirectory.GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }

        DateTime IDirectory.GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        string[] IDirectory.GetLogicalDrives()
        {
            throw new NotImplementedException();
        }

        IDirectoryInfo IDirectory.GetParent(string path)
        {
            throw new NotImplementedException();
        }

        void IDirectory.Move(string sourceDirName, string destDirName)
        {
            throw new NotImplementedException();
        }

        void IDirectory.SetAccessControl(string path, DirectorySecurity directorySecurity)
        {
            throw new NotImplementedException();
        }

        void IDirectory.SetCreationTime(string path, DateTime creationTime)
        {
            throw new NotImplementedException();
        }

        void IDirectory.SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            throw new NotImplementedException();
        }

        void IDirectory.SetCurrentDirectory(string path)
        {
            throw new NotImplementedException();
        }

        void IDirectory.SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        void IDirectory.SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        void IDirectory.SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        void IDirectory.SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }
    }

    public class TestFileSystem : IFileSystem
    {
        IFile _file = new TestFile();
        
        IDirectory _directory = new TestDirectory();

        public virtual IFile File { get { return _file; } }

        public virtual IDirectory Directory { get { return _directory; } }

        public IFileInfoFactory FileInfo => throw new NotImplementedException();

        public IFileStreamFactory FileStream => throw new NotImplementedException();

        public IPath Path => throw new NotImplementedException();

        public IDirectoryInfoFactory DirectoryInfo => throw new NotImplementedException();

        public IDriveInfoFactory DriveInfo => throw new NotImplementedException();

        public IFileSystemWatcherFactory FileSystemWatcher => throw new NotImplementedException();
    }
}
