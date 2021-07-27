using System.Collections.Generic;
using System.IO.Compression;

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
        /// Interval at which archive's existence will be polled. In milliseconds.
        /// </summary>
        int ArchiveAvailablePollInterval { get; set; }

        /// <summary>
        /// Time in seconds to wait for a package archive to build.
        /// </summary>
        int ArchiveWaitTimeout { get; set; }

        /// <summary>
        /// If true, a package will be automatically archived after package creation.
        /// </summary>
        bool AutoCreateArchiveOnPackageCreate { get; set; }

        /// <summary>
        /// Time in milliseconds to wait for a locked link to be released.
        /// </summary>
        int LinkLockWaitTime { get; set; }

        /// <summary>
        /// Time in seconds for objects in cache to timeout.
        /// </summary>
        int CacheTimeout { get; set; }

        /// <summary>
        /// Number of packages to list on index page.
        /// </summary>
        int ListPageSize { get; set; }

        /// <summary>
        /// Number of tags to list on index page
        /// </summary>
        int IndexTagListLength { get; set; }

        /// <summary>
        /// Number of page links to display at a time on pager bar.
        /// </summary>
        int PagesPerPageGroup { get; set; }

        /// <summary>
        /// Maximum number of archives to allow - once exceeded, older archives will be autodeleted
        /// </summary>
        int MaxArchives { get; set; }

        /// <summary>
        /// Minimum amount of free space (megabytes) on storage drive - if less is available, new uploads will fail.
        /// </summary>
        long SpaceSafetyThreshold { get; set; }

        /// <summary>
        /// The required auth level a user needs for the current Tetrifact instance.
        /// </summary>
        AuthorizationLevel AuthorizationLevel { get; set; }

        /// <summary>
        /// Collection of tokens which provide write access to 
        /// </summary>
        IEnumerable<string> AccessTokens { get; set; }

        /// <summary>
        /// If true, package data will be compressed on storage. This will consume less disk space, but required more CPU power to read packages.
        /// </summary>
        bool IsStorageCompressionEnabled { get; set; }

        /// <summary>
        /// Zip compression for downloadable builds. Set via Env var with values 0-2
        /// </summary>
        CompressionLevel DownloadArchiveCompression { get; set; }
    }
}
