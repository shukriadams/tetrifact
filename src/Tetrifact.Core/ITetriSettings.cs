using System.Collections.Generic;

namespace Tetrifact.Core
{
    public interface ITetriSettings
    {

        /// <summary>
        /// Root folder for projects. This is a child of the main data folder, and contains a folder for each
        /// project on the server. A "default" project will be autocreated, and user-specified projects can be
        /// set up next to "default".
        /// </summary>
        string ProjectsPath { get; set; }

        /// <summary>
        /// Path where global logs are stored.
        /// </summary>
        string LogPath { get; set; }

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
    }
}
