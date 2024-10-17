using System.Collections.Generic;
using System.IO.Compression;

namespace Tetrifact.Core
{
    public interface ISettings
    {
        /// <summary>
        /// Directory archive generation files are stored in. Wiped on app start.
        /// </summary>
        string ArchiveQueuePath { get; set; }

        /// <summary>
        /// Name of server, displayed in layout. Cosmetic.
        /// </summary>
        string ServerName { get; set; }

        /// <summary>
        /// Secondary title in header. Cosmetic.
        /// </summary>
        string ServerSecondaryName { get; set; }

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
        /// for debugging on production - if false, prune logic will run, but actual prune deletes will be suppressed.
        /// </summary>
        bool PruneDeletesEnabled { get; set; }

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
        int MaximumArchivesToKeep { get; set; }

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
        bool StorageCompressionEnabled { get; set; }

        /// <summary>
        /// Zip compression for downloadable builds. Allowed values Optimal | Fastest | NoCompression | SmallestSize.
        /// </summary>
        CompressionLevel ArchiveCompression { get; set; }

        /// <summary>
        /// If true, package autoprune will run
        /// </summary>
        bool PruneEnabled { get; set; }

        /// <summary>
        /// Nr of threads to spread worker processes over where possible
        /// </summary>
        int WorkerThreadCount { get; set; }

        /// <summary>
        /// List of tags which will not be pruned.
        /// </summary>
        IEnumerable<string> PruneIgnoreTags { get; set; }

        /// <summary>
        /// If false, existing packages cannot be deleted from UI or API.
        /// </summary>
        bool PackageDeleteEnabled { get; set; }

        /// <summary>
        /// If false, package uploads from UI or API will not be allowed.
        /// </summary>
        bool PackageCreateEnabled { get; set; }

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
        /// Cron mask for running metrics generation.
        /// </summary>
        string MetricsCronMask { get; set; }

        /// <summary>
        /// Time (hours) metric regeneration is allowed to be late before an error is flagged
        /// </summary>
        int MetricsGenerationBufferTime { get; set; }

        /// <summary>
        /// If true, Tetrifact's temp directory will be purged on start. This is highly recommended, as it cleans out hanging archives
        /// and other junk.
        /// </summary>
        bool WipeTempOnStart { get; set; }

        /// <summary>
        /// Switch for archive method. Default is dotnet zip compression. Allowed values are 
        /// </summary>
        ArchivingModes ArchivingMode { get; set; }

        /// <summary>
        /// Nr of threads to use for archiving process, where archive supports multithreading.
        /// </summary>
        int ArchiveCPUThreads { get; set; }

        /// <summary>
        /// Time brackets for auto-deleting packages. Comma-separated DAYSd COUNT
        /// Egs "5d 10, 20d 2, 10d 0"
        /// </summary>
        IEnumerable<PruneBracket> PruneBrackets {get; set; }

        /// <summary>
        /// Location on disk that app settings is located
        /// </summary>
        string SettingsPath { get; set; }

        /// <summary>
        /// Theme modifier appearance. Allowed values are dark|<empty string>.
        /// </summary>
        string Theme { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IEnumerable<TagColor> TagColors { get; set; }
    }
}
