using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Tetrifact.Core
{
    public class Settings : ISettings
    {
        #region FIELDS

        private readonly ILogger<ISettings> _log;

        #endregion

        #region PROPERTIES

        public string PackagePath { get; set; }

        public string TempPath { get; set; }

        public string RepositoryPath { get; set; }

        public string ArchivePath { get; set; }

        public string TagsPath { get; set; }

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

        public string PruneIgnoreTags { get; set; }

        public int PruneMonthlyThreshold { get; set; }

        public int PruneMonthlyKeep { get; set; }

        public int PruneWeeklyThreshold { get; set; }

        public int PruneWeeklyKeep { get; set; }

        public int PruneYearlyThreshold { get; set; }

        public int PruneYearlyKeep { get; set; }

        public int WorkerThreadCount { get; set; }

        #endregion

        #region CTORS

        public Settings(ILogger<ISettings> log)
        {
            _log = log;

            // defaults
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
            this.IsStorageCompressionEnabled = false;
            this.DownloadArchiveCompression = CompressionLevel.Optimal;
            this.PruneIgnoreTags = string.Empty;
            this.PruneWeeklyKeep = 7; 
            this.PruneMonthlyKeep = 4;
            this.PruneYearlyKeep = 12;
            this.PruneWeeklyThreshold = 21; // 3 weeks for weekly prune to kick in
            this.PruneMonthlyThreshold = 90; // circa 3 months for monthly prune to kick in
            this.PruneYearlyThreshold = 365; // circa 1 year for yearly prune to kick in, this applies to all packages after that
            this.WorkerThreadCount = 8;

            // get settings from env variables
            this.PackagePath = Environment.GetEnvironmentVariable("PACKAGE_PATH");
            this.TempPath = Environment.GetEnvironmentVariable("TEMP_PATH");
            this.RepositoryPath = Environment.GetEnvironmentVariable("HASH_INDEX_PATH");
            this.ArchivePath = Environment.GetEnvironmentVariable("ARCHIVE_PATH");
            this.TagsPath = Environment.GetEnvironmentVariable("TAGS_PATH");
            this.PackageDiffsPath = Environment.GetEnvironmentVariable("PACKAGE_DIFFS_PATH");

            this.IsStorageCompressionEnabled = this.GetSetting("STORAGE_COMPRESSION", this.IsStorageCompressionEnabled);
            this.Prune = this.GetSetting("PRUNE", this.Prune);
            this.PruneIgnoreTags = this.GetSetting("PRUNE_IGNORE_TAGS", this.PruneIgnoreTags);
            this.PruneWeeklyThreshold = this.GetSetting("PRUNE_WEEKLY_THRESHOLD", this.PruneWeeklyThreshold);
            this.PruneWeeklyKeep = this.GetSetting("PRUNE_WEEKLY_KEEP", this.PruneWeeklyKeep);
            this.PruneMonthlyThreshold = this.GetSetting("PRUNE_MONTHLY_THRESHOLD", this.PruneMonthlyThreshold);
            this.PruneMonthlyKeep = this.GetSetting("PRUNE_MONTHLY_KEEP", this.PruneMonthlyKeep);
            this.PruneYearlyThreshold = this.GetSetting("PRUNE_YEARLY_THRESHOLD", this.PruneYearlyThreshold);
            this.PruneYearlyKeep = this.GetSetting("PRUNE_YEARLY_KEEP", this.PruneYearlyKeep);
            
            this.WorkerThreadCount = this.GetSetting("WORKER_THREAD_COUNT", this.WorkerThreadCount);
            this.ListPageSize = this.GetSetting("LIST_PAGE_SIZE", this.ListPageSize);
            this.MaxArchives = this.GetSetting("MAX_ARCHIVES", this.MaxArchives);
            this.AuthorizationLevel = this.GetSetting("AUTH_LEVEL", this.AuthorizationLevel);
            this.SpaceSafetyThreshold = this.GetSetting("SPACE_SAFETY_THRESHOLD", this.SpaceSafetyThreshold);
            this.AutoCreateArchiveOnPackageCreate = this.GetSetting("AUTO_CREATE_ARCHIVE_ON_PACKAGE_CREATE", this.AutoCreateArchiveOnPackageCreate);

            string downloadArchiveCompressionEnvVar = Environment.GetEnvironmentVariable("DOWNLOAD_ARCHIVE_COMPRESSION");
            if (!string.IsNullOrEmpty(downloadArchiveCompressionEnvVar)){ 
                if (downloadArchiveCompressionEnvVar == "0")
                    DownloadArchiveCompression = CompressionLevel.NoCompression;
                if (downloadArchiveCompressionEnvVar == "1")
                    DownloadArchiveCompression = CompressionLevel.Fastest;
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACCESS_TOKENS"))) 
                this.AccessTokens = Environment.GetEnvironmentVariable("ACCESS_TOKENS").Split(",");

            // fall back to defaults
            if (string.IsNullOrEmpty(PackageDiffsPath))
                PackageDiffsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packageDiffs");

            if (string.IsNullOrEmpty(PackagePath))
                PackagePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packages");

            if (string.IsNullOrEmpty(TempPath))
                TempPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "temp");

            if (string.IsNullOrEmpty(RepositoryPath))
                RepositoryPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "repository");

            if (string.IsNullOrEmpty(ArchivePath))
                ArchivePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archives");

            if (string.IsNullOrEmpty(TagsPath))
                TagsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "tags");
        }

        /// <summary>
        /// Safely gets integer setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private int GetSetting(string settingsName, int defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            if (!int.TryParse(settingsRawVariable, out defaultValue))
                _log.LogError($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid integer.");

            return defaultValue;
        }

        /// <summary>
        /// Safely gets long setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>

        private long GetSetting(string settingsName, long defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            if (!long.TryParse(settingsRawVariable, out defaultValue))
                _log.LogError($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid long.");

            return defaultValue;
        }

        private bool GetSetting(string settingsName, bool defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            if (!Boolean.TryParse(settingsRawVariable, out defaultValue))
                _log.LogError($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid boolean.");

            return defaultValue;
        }

        /// <summary>
        /// Safely gets enum setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private TEnum GetSetting<TEnum>(string settingsName, TEnum defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            // messy using try/catch instead of TryParse, but I can't figure out enum tryparse with generics
            try
            {
                defaultValue = (TEnum)Enum.Parse(typeof(TEnum), settingsRawVariable);
            }
            catch
            {
                _log.LogError($"Environment variable for {settingsName} ({settingsRawVariable}) is invalid, it must match one of {string.Join(",", Enum.GetNames(typeof(TEnum)))}.");
            }

            return defaultValue;
        }

        #endregion
    }
}
