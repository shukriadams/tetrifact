namespace Tetrifact.Core
{
    public interface ITetriSettings
    {
        /// <summary>
        /// Path where packages are stored. Each package lives in its own folder.
        /// </summary>
        string PackagePath { get; set; }

        /// <summary>
        /// Folder to store incoming files, temp archives, etc. Wiped on app start.
        /// </summary>
        string TempPath { get; set; }
 
        /// <summary>
        /// Folder to store complete archives. Each archive is named for the package it contains.
        /// </summary>
        string ArchivePath { get; set; }

        /// <summary>
        /// Global path to hash index - this cross-package index is used to determine which package contains a specific hash.
        /// </summary>
        string RepositoryPath { get; set; }

        /// <summary>
        /// Folder tags are written to.
        /// </summary>
        string TagsPath { get; set; }

        /// <summary>
        /// Milliseconds.
        /// </summary>
        int ArchiveAvailablePollInterval { get; set; }

        /// <summary>
        /// Time in seconds to wait for a package archive to build.
        /// </summary>
        int ArchiveWaitTimeout { get; set; }

        /// <summary>
        /// Time in seconds for objects in cache to timeout.
        /// </summary>
        int CacheTimeout { get; set; }

        /// <summary>
        /// Number of packages to list on index page.
        /// </summary>
        int IndexPackageListLength { get; set; }

        /// <summary>
        /// Number of tags to list on index page
        /// </summary>
        int IndexTagListLength { get; set; }
    }
}
