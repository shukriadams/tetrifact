using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

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

        public int MaximumArchivesToKeep { get; set; }

        public long SpaceSafetyThreshold { get; set; }

        public AuthorizationLevel AuthorizationLevel { get; set; }

        public IEnumerable<string> AccessTokens { get; set; }
        
        public bool StorageCompressionEnabled { get; set; }

        public bool AutoCreateArchiveOnPackageCreate { get; set; }
        
        public CompressionLevel ArchiveCompression { get; set; }

        public bool PruneEnabled { get; set; }

        public int WorkerThreadCount { get; set; }

        public IEnumerable<string> PruneIgnoreTags { get; set; }

        public  bool PackageDeleteEnabled { get; set; }

        public  bool PackageCreateEnabled { get; set; }

        public int MetricsGenerationInterval { get; set; }

        public string ServerName { get; set; }

        public string ServerSecondaryName { get; set; }

        public string LogPath { get; set; }

        public string ArchiveQueuePath { get; set; }

        public string CleanCronMask { get; set; }

        public string PruneCronMask { get; set; }

        public string MetricsCronMask { get; set; }

        public bool PruneDeletesEnabled { get ;set; }

        public int MetricsGenerationBufferTime { get; set; }

        public bool WipeTempOnStart { get; set; }

        public ArchivingModes ArchivingMode { get; set; }

        public int ArchiveCPUThreads { get; set; }

        public IEnumerable<PruneBracket> PruneBrackets { get; set; }

        public string SettingsPath { get; set; }

        public int? MaximumSimultaneousDownloads { get; set; }

        public int DownloadQueueTicketLifespan { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Theme { get; set; }

        public IEnumerable<TagColor> TagColors { get; set; }

        #endregion

        #region CTORS

        public Settings()
        {
            // defaults
            this.DownloadQueueTicketLifespan = 20; // seconds
            this.AccessTokens = new List<string>();
            this.PackageDeleteEnabled = true;
            this.PackageCreateEnabled = true;
            this.ArchiveCPUThreads = 4;                 // for compression solutions that support multithreading.
            this.ArchivingMode = ArchivingModes.Default;   // default dotnet zip compression
            this.ArchiveAvailablePollInterval = 1000;   // 1 second
            this.ArchiveWaitTimeout = 10 * 60;          // 10 minutes
            this.ArchiveQueuePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archiveQueue");
            this.ArchivePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archives");
            this.AuthorizationLevel = AuthorizationLevel.None;
            this.CacheTimeout = 60 * 60;                // 1 hour
            this.CleanCronMask = "0 0 * * *"; // once a day at midnight
            this.ArchiveCompression = CompressionLevel.Optimal;
            this.StorageCompressionEnabled = false;
            this.LinkLockWaitTime = 1000;               // 1 second
            this.ListPageSize = 20;
            this.LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs", "log.txt");
            this.IndexTagListLength = 20;
            this.MaximumArchivesToKeep = 10;
            this.MetricsCronMask = "0 4 * * *"; // once a day at 4 am
            this.MetricsGenerationBufferTime = 1; // 1 hour
            this.MetricsGenerationInterval = 24;
            this.MetricsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "metrics");
            this.PackageDiffsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packageDiffs");
            this.PackagePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packages");
            this.PagesPerPageGroup = 10;
            this.PruneIgnoreTags = new string[] { };
            this.PruneDeletesEnabled = true;
            this.PruneCronMask = "0 2 * * *"; // once a day at 2 am
            this.PruneBrackets = new List<PruneBracket>();
            this.RepositoryPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "repository");
            this.ServerName = "Tetrifact";
            this.ServerSecondaryName = "Artefact Storage";
            this.SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.yml");
            this.TagColors = new List<TagColor>();
            this.TagsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "tags");
            this.TempPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "temp");
            this.Theme = "dark";
            this.WipeTempOnStart = true;
            this.WorkerThreadCount = 8;
        }

        #endregion

        #region METHODS

        public bool Validate()
        {
            bool isValid = true;

            // detect overlapping prune brackets 
            IList<IGrouping<int, PruneBracket>> duplicateGroups = this.PruneBrackets.GroupBy(p => p.Days).Where(g => g.Count() > 1).ToList();
            if (duplicateGroups.Count > 0)
            {
                foreach (IGrouping<int, PruneBracket> duplicateGroup in duplicateGroups) 
                    Console.WriteLine($"CONFIG ERROR : Prune bracket day {duplicateGroup.Key} is defined twice.");

                isValid = false;
            }

            return isValid;
        }

        #endregion
    }
}
