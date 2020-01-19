using System;
using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public class Settings : ISettings
    {
        #region PROPERTIES

        public string ProjectsPath { get; set; }

        public string LogPath { get; set; }

        public string TempPath { get; set; }

        public string ArchivePath { get; set; }

        public string TempBinaries { get; set; }

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

        public int TransactionHistoryDepth { get; set; }

        public DiffMethods DiffMethod { get; set; }

        public int FilePersistTimeout { get; set; }
        
        public long FileChunkSize { get; set; }

        #endregion

        #region CTORS

        public Settings()
        {
            // defaults
            this.ArchiveAvailablePollInterval = 1000;   // 1 second
            this.ArchiveWaitTimeout = 10 * 60;          // 10 minutes
            this.LinkLockWaitTime = 1000;               // 1 second
            this.CacheTimeout = 60 * 60;                // 1 hour
            this.ListPageSize = 20;
            this.IndexTagListLength = 20;
            this.PagesPerPageGroup = 20;
            this.MaxArchives = 10;
            this.FilePersistTimeout = 10;               // days
            this.FileChunkSize = 1 * 1000000; // in bytes. remember, bytes * 1000000 = megabytes
            this.AuthorizationLevel = AuthorizationLevel.None;
            this.TransactionHistoryDepth = 2;
            this.DiffMethod = DiffMethods.VcDiff;   // VcDiff is about 5 times faster than BsDiff, hence default
            this.ProjectsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "projects");
            this.LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs", "log.txt");
            this.TempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "temp");
            this.ArchivePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "archives");
            this.TempBinaries = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "temp_binaries");


            // try to get settings from env variables
            this.ListPageSize = this.GetSetting("LIST_PAGE_SIZE", this.ListPageSize);
            this.FileChunkSize = this.GetSetting("FILE_CHUNK_SIZE", this.FileChunkSize);
            this.MaxArchives = this.GetSetting("MAX_ARCHIVES", this.MaxArchives);
            this.AuthorizationLevel = this.GetSetting("AUTH_LEVEL", this.AuthorizationLevel);
            this.SpaceSafetyThreshold = this.GetSetting("SPACE_SAFETY_THRESHOLD", this.SpaceSafetyThreshold);
            this.TransactionHistoryDepth = this.GetSetting("TRANSACTION_HISTORY_DEPTH", this.TransactionHistoryDepth);
            this.ProjectsPath = this.GetSetting("PROJECTS_PATH", this.ProjectsPath);
            this.LogPath = this.GetSetting("LOG_PATH", this.LogPath);
            this.TempPath = this.GetSetting("TEMP_PATH", this.TempPath);
            this.ArchivePath = this.GetSetting("ARCHIVE_PATH", this.ArchivePath);
            this.TempBinaries = this.GetSetting("TEMP_BINARIES", this.TempBinaries);
            this.FilePersistTimeout = this.GetSetting("FILE_PERSIST_TIMEOUT", this.FilePersistTimeout);

            // special case - access tokens can be passed in as a comma-separated string, need to split to array here
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACCESS_TOKENS"))) 
                this.AccessTokens = Environment.GetEnvironmentVariable("ACCESS_TOKENS").Split(",");

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DIFF_METHOD")))
                this.DiffMethod = (DiffMethods)Enum.Parse(typeof(DiffMethods), Environment.GetEnvironmentVariable("DIFF_METHOD").Trim());
        }

        /// <summary>
        /// Gets a string value from environment variable if that value exists. Else returns default value.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string GetSetting(string settingsName, string defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            return settingsRawVariable == null ? defaultValue : settingsRawVariable;
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
                throw new Exception($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid integer.");

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
                throw new Exception($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid integer.");

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
                throw new Exception($"Environment variable for {settingsName} ({settingsRawVariable}) is invalid, it must match one of {string.Join(",", Enum.GetNames(typeof(TEnum)))}.");
            }

            return defaultValue;
        }

        #endregion
    }
}
