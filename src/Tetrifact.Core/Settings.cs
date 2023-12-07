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

        public string CleanCronMask { get; set; }

        public string PruneCronMask { get; set; }

        public string MetricsCronMask { get; set; }

        public bool DEBUG_block_prune_deletes { get ;set; }

        public int MetricsGenerationBufferTime { get; set; }

        #endregion

        #region CTORS

        public Settings()
        {
            // defaults
            this.ServerName = "Tetrifact";
            this.AllowPackageDelete = true;
            this.AllowPackageCreate = true;
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
            this.LogLevel = "Warning";
            this.PackageDiffsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packageDiffs");
            this.PackagePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "packages");
            this.TempPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "temp");
            this.RepositoryPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "repository");
            this.ArchivePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "archives");
            this.TagsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "tags");
            this.MetricsPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "data", "metrics");
            this.CleanCronMask = "0 * * * *"; // ever hour on the hour
            this.PruneCronMask = "0 0 * * *"; // once a day at midnight
            this.MetricsCronMask = "0 */12 * * *"; // every 12 hours

            // try to overrride defaults from environment variables
            this.AllowPackageDelete = this.TryGetSetting("ALLOW_PACKAGE_DELETE", this.AllowPackageDelete);
            this.AllowPackageCreate = this.TryGetSetting("ALLOW_PACKAGE_CREATE", this.AllowPackageCreate);
            this.IsStorageCompressionEnabled = this.TryGetSetting("STORAGE_COMPRESSION", this.IsStorageCompressionEnabled);
            this.Prune = this.TryGetSetting("PRUNE", this.Prune);
            
            this.PruneWeeklyThreshold = this.TryGetSetting("PRUNE_WEEKLY_THRESHOLD", this.PruneWeeklyThreshold);
            this.PruneWeeklyKeep = this.TryGetSetting("PRUNE_WEEKLY_KEEP", this.PruneWeeklyKeep);
            this.PruneMonthlyThreshold = this.TryGetSetting("PRUNE_MONTHLY_THRESHOLD", this.PruneMonthlyThreshold);
            this.PruneMonthlyKeep = this.TryGetSetting("PRUNE_MONTHLY_KEEP", this.PruneMonthlyKeep);
            this.PruneYearlyThreshold = this.TryGetSetting("PRUNE_YEARLY_THRESHOLD", this.PruneYearlyThreshold);
            this.PruneYearlyKeep = this.TryGetSetting("PRUNE_YEARLY_KEEP", this.PruneYearlyKeep);
            this.MetricsGenerationInterval = this.TryGetSetting("METRICS_GENERATION_INTERVAL", this.MetricsGenerationInterval);
            this.ServerName = this.TryGetSetting("SERVER_NAME", this.ServerName);
            this.WorkerThreadCount = this.TryGetSetting("WORKER_THREAD_COUNT", this.WorkerThreadCount);
            this.ListPageSize = this.TryGetSetting("LIST_PAGE_SIZE", this.ListPageSize);
            this.MaxArchives = this.TryGetSetting("MAX_ARCHIVES", this.MaxArchives);
            this.AuthorizationLevel = this.TryGetSetting("AUTH_LEVEL", this.AuthorizationLevel);
            this.SpaceSafetyThreshold = this.TryGetSetting("SPACE_SAFETY_THRESHOLD", this.SpaceSafetyThreshold);
            this.AutoCreateArchiveOnPackageCreate = this.TryGetSetting("AUTO_CREATE_ARCHIVE_ON_PACKAGE_CREATE", this.AutoCreateArchiveOnPackageCreate);
            this.PruneIgnoreTags = this.TryGetSetting("PRUNE_IGNORE_TAGS", this.PruneIgnoreTags);
            this.LogPath = this.TryGetSetting("LOG_PATH", this.LogPath);
            this.LogLevel = this.TryGetSetting("Logging__LogLevel__System", this.LogLevel);
            this.PackageDiffsPath = this.TryGetSetting("PACKAGE_DIFFS_PATH", this.PackageDiffsPath);
            this.PackagePath = this.TryGetSetting("PACKAGE_PATH", this.PackagePath);
            this.TempPath = this.TryGetSetting("TEMP_PATH", this.TempPath);
            this.RepositoryPath = this.TryGetSetting("HASH_INDEX_PATH", this.RepositoryPath);
            this.ArchivePath = this.TryGetSetting("ARCHIVE_PATH", this.ArchivePath);
            this.TagsPath = this.TryGetSetting("TAGS_PATH", this.TagsPath);
            this.MetricsPath = this.TryGetSetting("METRICS_PATH", this.MetricsPath);
            this.CleanCronMask = this.TryGetSetting("CLEAN_CRON_MASK", this.CleanCronMask);
            this.PruneCronMask = this.TryGetSetting("PRUNE_CRON_MASK", this.PruneCronMask);
            this.MetricsPath = this.TryGetSetting("METRICS_CRON_MASK", this.MetricsPath);
            this.MetricsGenerationBufferTime = this.TryGetSetting("METRICS_GENERATION_BUFFER_TIME", this.MetricsGenerationBufferTime);
            this.DEBUG_block_prune_deletes = this.TryGetSetting("DEBUG_BLOCK_PRUNE_DELETES", this.DEBUG_block_prune_deletes);

            string downloadArchiveCompressionEnvVar = Environment.GetEnvironmentVariable("DOWNLOAD_ARCHIVE_COMPRESSION");
            if (downloadArchiveCompressionEnvVar == "0")
                DownloadArchiveCompression = CompressionLevel.NoCompression;
            if (downloadArchiveCompressionEnvVar == "1")
                DownloadArchiveCompression = CompressionLevel.Fastest;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACCESS_TOKENS"))) 
                this.AccessTokens = Environment.GetEnvironmentVariable("ACCESS_TOKENS").Split(",");
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Gets a string environment variable if it is defined.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string TryGetSetting(string settingsName, string defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            
            if (settingsRawVariable == null)
                return defaultValue;
                
            return settingsRawVariable;
        }

        /// <summary>
        /// Safely gets integer setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private int TryGetSetting(string settingsName, int defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            int attempt;
            if (!int.TryParse(settingsRawVariable, out attempt)){
                Console.WriteLine($"WARNING: Environment variable for {settingsName} ({settingsRawVariable}) is not a valid integer.");
                return defaultValue;
            }

            return attempt;
        }

        /// <summary>
        /// Safely gets long setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private long TryGetSetting(string settingsName, long defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            long attempt;
            if (!long.TryParse(settingsRawVariable, out attempt)){
                Console.WriteLine($"WARNING: Environment variable for {settingsName} ({settingsRawVariable}) is not a valid long.");
                return defaultValue;
            }

            return attempt;
        }

        private bool TryGetSetting(string settingsName, bool defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            bool attempt;
            if (!Boolean.TryParse(settingsRawVariable, out attempt)){
                Console.WriteLine($"WARNING: Environment variable for {settingsName} ({settingsRawVariable}) is not a valid boolean.");
                return defaultValue;
            }

            return attempt;
        }

        /// <summary>
        /// Gets an array of values from comma-separated string
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private IEnumerable<string> TryGetSetting(string settingsName, IEnumerable<string> defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (string.IsNullOrEmpty(settingsRawVariable))
                return defaultValue;

            return settingsRawVariable.Split(",",StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Safely gets enum setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private TEnum TryGetSetting<TEnum>(string settingsName, TEnum defaultValue)
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
                Console.WriteLine($"WARNING: Environment variable for {settingsName} ({settingsRawVariable}) is invalid, it must match one of {string.Join(",", Enum.GetNames(typeof(TEnum)))}.");
            }

            return defaultValue;
        }

        #endregion
    }
}
