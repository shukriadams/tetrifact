using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    /// <summary>
    /// Wraps the placement of an incoming package. This can be either a folder on a file system, or for testing purposes, in-memory collections.
    /// </summary>
    public interface IWorkspace
    {
        /// <summary>
        /// Starts workspace. Must be called before anything can be added to workspace.
        /// </summary>
        void Initialize(string project);

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
        void AddIncomingArchive(Stream file);

        /// <summary>
        /// Returns a list of all file names in incoming folder.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetIncomingFileNames();

        /// <summary>
        /// Moves all files from incoming to staging folder, or writes a diff patch to staging folder if a previous version of file exists.  
        /// </summary>
        /// <param name="packageId"></param>
        void StageAllFiles(string packageId);

        /// <summary>
        /// Writes the final manfiest for the package. If applicabale, writes manifest object as a JSON file. also updated head.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="package"></param>
        void Finalize(string project, string package, string diffAgainstPackage);

        /// <summary>
        /// Gets the hash of an incoming file. On filesystems, this is done by reading the file directly and hashing it.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetIncomingFileHash(string path);

        /// <summary>
        /// Cleans up workspace.
        /// </summary>
        void Dispose();
    }
}
