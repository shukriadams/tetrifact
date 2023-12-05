using System.Collections.Generic;
using System.IO.Compression;

namespace Tetrifact.Core
{
    public interface ISettings
    {
        /// <summary>
        /// Name of server, displayed in layout. Cosmetic only
        /// </summary>
        string ServerName { get; set; }

        /// <summary>
        /// Path where packages are stored. Each package lives in its own directory.
        /// </summary>
        string PackagePath { get; set; }

        /// <summary>
        /// Directory to store incoming files, temp archives, etc. Wiped on app start.
        /// </summary>
        string TempPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string LogPath { get; set; }

        /// <summary>
        /// Allowed values are: Information|Warning|Error
        /// </summary>
        string LogLevel { get; set; }

        /// <summary>
        /// Directory to store complete archives. Each archive is named for the package it contains.
        /// </summary>
        string ArchivePath { get; set; }

        /// <summary>
        /// Global path to hash index - this cross-package index is used to determine which package contains a specific hash.
        /// </summary>
        string RepositoryPath { get; set; }

        /// <summary>
        /// Directory tags are written to.
        /// </summary>
        string TagsPath { get; set; }

        /// <summary>
        /// Directory metric data is cached to
        /// </summary>
        string MetricsPath { get; set; }

        /// <summary>
        /// Directory where diffs between packages are written to.
        /// </summary>
        string PackageDiffsPath { get; set; }

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
        /// for debugging on production - if true, prune logic will run, but actual prune deletes will be suppressed.
        /// </summary>
        bool DEBUG_block_prune_deletes { get; set; }

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

        /// <summary>
        /// If true, package autoprune will run
        /// </summary>
        bool Prune { get; set; }
        
        /// <summary>
        /// Minimum age (in days) for a package to be applicable for monthly prune 
        /// </summary>
        int PruneMonthlyThreshold { get; set; }

        /// <summary>
        /// Number of packages to keep per month. 
        /// </summary>
        int PruneMonthlyKeep { get; set; }

        /// <summary>
        /// Minimum age (in days) for a package to be applicable for weekly prune 
        /// </summary>
        int PruneWeeklyThreshold { get; set; }

        /// <summary>
        /// Number of packages to keep per week.
        /// </summary>
        int PruneWeeklyKeep { get; set; }

        /// <summary>
        /// Minimum age (in days) for a package to be applicable for yearly prune 
        /// </summary>
        int PruneYearlyThreshold { get; set; }

        /// <summary>
        /// Number of packages to keep per year. 
        /// </summary>
        int PruneYearlyKeep { get; set; }

        /// <summary>
        /// Nr of threads to spread worker processes over where possible
        /// </summary>
        int WorkerThreadCount { get; set; }

        /// <summary>
        /// Comma-separated list of tags which will not be pruned.
        /// </summary>
        IEnumerable<string> PruneIgnoreTags { get; set; }

        /// <summary>
        /// If false, existing packages cannot be deleted from UI or API.
        /// </summary>
        bool AllowPackageDelete { get; set; }

        /// <summary>
        /// If false, package uploads from UI or API will not be allowed.
        /// </summary>
        bool AllowPackageCreate { get; set; }

        /// <summary>
        /// Interval in hours for metrics to be regenerated.
        /// </summary>
        int MetricsGenerationInterval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string CleanCronMask { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string PruneCronMask { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string MetricsCronMask { get; set; }

        /// <summary>
        /// Time (hours) metric regeneration is allowed to be late before an error is flagged
        /// </summary>
        int MetricsGenerationBufferTime { get; set; }
    }
}
