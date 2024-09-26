using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Tetrifact.Core
{
    public class Settings : ISettings
    {
        #region PROPERTIES

        public string PackagePath { get; set; }

        public string TempPath { get; set; }

        public string RepositoryPath { get; set; }

        public string ArchivePath { get; set; }

        public string TagsPath { get; set; }

        public string MetricsPath { get; set; }

        public string PackageDiffsPath { get; set; }

        public int ArchiveAvailablePollInterval { get; set; }

        public int ArchiveWaitTimeout { get; set; }

        public int ListPageSize { get; set; }

        public int IndexTagListLength { get; set; }

        public int PagesPerPageGroup { get; set; }

        public int CacheTimeout { get; set; }

        public int LinkLockWaitTime { get; set; }

        public int MaxArchives { get; set; }

        public long SpaceSafetyThreshold { get; set; }

        public AuthorizationLevel AuthorizationLevel { get; set; }

        public IEnumerable<string> AccessTokens { get; set; }
        
        public bool IsStorageCompressionEnabled { get; set; }

        public bool AutoCreateArchiveOnPackageCreate { get; set; }
        
        public CompressionLevel DownloadArchiveCompression { get; set; }

        public bool Prune { get; set; }

        public int PruneMonthlyThreshold { get; set; }

        public int PruneMonthlyKeep { get; set; }

        public int PruneWeeklyThreshold { get; set; }

        public int PruneWeeklyKeep { get; set; }

        public int PruneYearlyThreshold { get; set; }

        public int PruneYearlyKeep { get; set; }

        public int WorkerThreadCount { get; set; }

        public IEnumerable<string> PruneIgnoreTags { get; set; }

        public  bool AllowPackageDelete { get; set; }

        public  bool AllowPackageCreate { get; set; }

        public int MetricsGenerationInterval { get; set; }

        public string ServerName { get; set; }

        public string ServerSecondaryName { get; set; }

        public string LogPath { get; set; }

        public string LogLevel{ get; set; }

        public string ArchiveQueuePath { get; set; }

        public string CleanCronMask { get; set; }

        public string PruneCronMask { get; set; }

        public string MetricsCronMask { get; set; }

        public bool DEBUG_block_prune_deletes { get ;set; }

        public int MetricsGenerationBufferTime { get; set; }

        public bool WipeTempOnStart { get; set; }

        public string ExternaArchivingExecutable { get; set; }

        public ArchivingModes ArchivingMode { get; set; }

        public int ArchiveCPUThreads { get; set; }

        public IEnumerable<PruneBracket> PruneBrackets { get; set; }

        public string SettingsPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ThemeClass { get; set; }

        public IEnumerable<TagColor> TagColors { get; set; }

        #endregion

        #region CTORS

        public Settings()
        {
            // defaults
            this.AccessTokens = new List<string>();
            this.AllowPackageDelete = true;
            this.AllowPackageCreate = true;
            this.ArchiveCPUThreads = 4;                 // for compression solutions that 7zip only
            this.ArchivingMode = ArchivingModes.Internal;   // default dotnet zip compression
            this.ArchiveAvailablePollInterval = 1000;   // 1 second
            this.ArchiveWaitTimeout = 10 * 60;          // 10 minutes
            this.ArchiveQueuePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archiveQueue");
            this.ArchivePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archives");
            this.AuthorizationLevel = AuthorizationLevel.None;
            this.CacheTimeout = 60 * 60;                // 1 hour
            this.CleanCronMask = "0 0 * * *"; // once a day at midnight
            this.DownloadArchiveCompression = CompressionLevel.Optimal;
            this.IsStorageCompressionEnabled = false;
            this.LinkLockWaitTime = 1000;               // 1 second
            this.ListPageSize = 20;
            this.LogLevel = "Warning";
            this.LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs", "log.txt");
            this.IndexTagListLength = 20;
            this.MaxArchives = 10;
            this.MetricsCronMask = "0 4 * * *"; // once a day at 4 am
            this.MetricsGenerationBufferTime = 1; // 1 hour
            this.MetricsGenerationInterval = 24;
            this.MetricsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "metrics");
            this.PackageDiffsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packageDiffs");
            this.PackagePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packages");
            this.PagesPerPageGroup = 10;
            this.PruneWeeklyKeep = 7; 
            this.PruneMonthlyKeep = 4;
            this.PruneYearlyKeep = 12;
            this.PruneWeeklyThreshold = 21; // 3 weeks for weekly prune to kick in
            this.PruneMonthlyThreshold = 90; // circa 3 months for monthly prune to kick in
            this.PruneYearlyThreshold = 365; // circa 1 year for yearly prune to kick in, this applies to all packages after that
            this.PruneIgnoreTags = new string[] { };
            this.PruneCronMask = "0 2 * * *"; // once a day at 2 am
            this.PruneBrackets = new List<PruneBracket>();
            this.RepositoryPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "repository");
            this.ServerName = "Tetrifact";
            this.ServerSecondaryName = "Artefact Storage";
            this.SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.yml");
            this.TagColors = new List<TagColor>();
            this.TagsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "tags");
            this.TempPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "temp");
            this.ThemeClass = "dark";
            this.WipeTempOnStart = true;
            this.WorkerThreadCount = 8;
        }

        #endregion
    }
}
