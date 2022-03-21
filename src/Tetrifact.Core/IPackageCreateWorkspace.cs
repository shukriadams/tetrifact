using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// Wraps the placement of an incoming package. This can be either a folder on a file system, or for testing purposes, in-memory collections.
    /// </summary>
    public interface IPackageCreateWorkspace
    {
        /// <summary>
        /// Starts workspace. Must be called before anything can be added to workspace.
        /// </summary>
        void Initialize();

        /// <summary>
        /// A manifest generated for the incoming package. If on the filesystem, can be written as a json file.
        /// </summary>
        Manifest Manifest { get; }

        /// <summary>
        /// The absolute path of the workspace folder.
        /// </summary>
        string WorkspacePath { get; }

        /// <summary>
        /// Add a single file to the packages's incoming folder. Stream can be sourced from a POSTed IFormfile, or a generic data stream.
        /// </summary>
        /// <param name="file"></param>
        bool AddIncomingFile(Stream file, string relativePath);

        /// <summary>
        /// Adds the contents of an archive to the package's incoming folder. Must be a valid archive stream. Stream can be sourced from a POSTed IFormfile, or a generic data stream.
        /// </summary>
        /// <param name="file"></param>
        void AddArchiveContent(Stream file);

        /// <summary>
        /// Returns a list of all file names in incoming folder.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetIncomingFileNames();

        /// <summary>
        /// Moves a file from incoming to respository folder. This file will immediately be directly accessible if looked for in the repo.
        /// </summary>
        /// <param name="fileInIncoming"></param>
        /// <param name="hash"></param>
        /// <param name="packageId"></param>
        /// <param name="enableCompression"></param>
        void WriteFile(string fileInIncoming, string hash, long fileSize, string packageId);

        /// <summary>
        /// Writes the final manfiest for the package. If applicabale, writes manifest object as a JSON file.
        /// </summary>
        /// <param name="combinedHash"></param>
        void WriteManifest(string packageId, string combinedHash);

        /// <summary>
        /// Gets the hash of an incoming file. On filesystems, this is done by reading the file directly and hashing it.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        (string, long) GetIncomingFileProperties(string path);

        /// <summary>
        /// Cleans up workspace.
        /// </summary>
        void Dispose();
    }
}
