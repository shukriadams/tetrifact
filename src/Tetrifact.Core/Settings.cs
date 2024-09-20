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

        public string LogPath { get; set; }

        public string LogLevel{ get; set; }

        public string ArchiveQueuePath { get; set; }

        public string CleanCronMask { get; set; }

        public string PruneCronMask { get; set; }

        public string MetricsCronMask { get; set; }

        public bool DEBUG_block_prune_deletes { get ;set; }

        public int MetricsGenerationBufferTime { get; set; }

        public bool WipeTempOnStart { get; set; }

        public string SevenZipBinaryPath { get; set; }

        public int ArchiveCPUThreads { get; set; }

        public IEnumerable<PruneBracket> PruneBrackets { get; set; }

        public string SettingsPath { get; set; }

        public IEnumerable<TagColor> TagColors { get; set; }

        #endregion

        #region CTORS

        public Settings()
        {
            // defaults
            this.ArchiveCPUThreads = 4;                 // for 7zip only
            this.ServerName = "Tetrifact";
            this.AllowPackageDelete = true;
            this.AllowPackageCreate = true;
            this.WipeTempOnStart = true;
            this.ArchiveAvailablePollInterval = 1000;   // 1 second
            this.ArchiveWaitTimeout = 10 * 60;          // 10 minutes
            this.LinkLockWaitTime = 1000;               // 1 second
            this.CacheTimeout = 60 * 60;                // 1 hour
            this.ListPageSize = 20;
            this.IndexTagListLength = 20;
            this.PagesPerPageGroup = 10;
            this.MaxArchives = 10;
            this.AuthorizationLevel = AuthorizationLevel.None;
            this.AccessTokens = new List<string>();
            this.PruneBrackets = new List<PruneBracket>();
            this.TagColors = new List<TagColor>();
            this.IsStorageCompressionEnabled = false;
            this.DownloadArchiveCompression = CompressionLevel.Optimal;
            this.PruneWeeklyKeep = 7; 
            this.PruneMonthlyKeep = 4;
            this.PruneYearlyKeep = 12;
            this.PruneWeeklyThreshold = 21; // 3 weeks for weekly prune to kick in
            this.PruneMonthlyThreshold = 90; // circa 3 months for monthly prune to kick in
            this.PruneYearlyThreshold = 365; // circa 1 year for yearly prune to kick in, this applies to all packages after that
            this.WorkerThreadCount = 8;
            this.MetricsGenerationInterval = 24;
            this.PruneIgnoreTags = new string[] { };
            this.MetricsGenerationBufferTime = 1; // 1 hour
            this.LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs", "log.txt");

            this.SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.yml");
            this.LogLevel = "Warning";
            this.PackageDiffsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packageDiffs");
            this.PackagePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packages");
            this.TempPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "temp");
            this.RepositoryPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "repository");
            this.ArchiveQueuePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archiveQueue");
            this.ArchivePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archives");
            this.TagsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "tags");
            this.MetricsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "metrics");
            this.CleanCronMask = "0 0 * * *"; // once a day at midnight
            this.PruneCronMask = "0 2 * * *"; // once a day at 2 am
            this.MetricsCronMask = "0 4 * * *"; // once a day at 4 am
        }

        #endregion
    }
}
