using System;
using System.Collections.Generic;
using System.IO;

namespace Tetrifact.Core
{
    public static class Settings
    {
        #region PROPERTIES

        /// <summary>
        /// Root folder for projects. This is a child of the main data folder, and contains a folder for each
        /// project on the server. A "default" project will be autocreated, and user-specified projects can be
        /// set up next to "default".
        /// </summary>
        public static string ProjectsPath { get; set; }

        /// <summary>
        /// Path where global logs are stored.
        /// </summary>
        public static string LogPath { get; set; }

        /// <summary>
        /// Folder to store incoming files, temp archives, etc. Wiped on app start.
        /// </summary>
        public static string TempPath { get; set; }

        /// <summary>
        /// Folder to store complete archives. Each archive is named for the project + package it contains. Archives are global
        /// across all projects.
        /// </summary>
        public static string ArchivePath { get; set; }

        /// <summary>
        /// Folder where patched binary files are stored.
        /// </summary>
        public static string TempBinaries { get; set; }

        /// <summary>
        /// Interval at which archive's existence will be polled. In milliseconds.
        /// </summary>
        public static int ArchiveAvailablePollInterval { get; set; }

        /// <summary>
        /// Time in seconds to wait for a package archive to build.
        /// </summary>
        public static int ArchiveWaitTimeout { get; set; }

        /// <summary>
        /// Number of packages to list on index page.
        /// </summary>
        public static int ListPageSize { get; set; }

        /// <summary>
        /// Number of tags to list on index page
        /// </summary>
        public static int IndexTagListLength { get; set; }

        /// <summary>
        /// Number of page links to display at a time on pager bar.
        /// </summary>
        public static int PagesPerPageGroup { get; set; }

        /// <summary>
        /// Time in seconds for objects in cache to timeout.
        /// </summary>
        public static int CacheTimeout { get; set; }

        /// <summary>
        /// Time in milliseconds to wait for a locked link to be released.
        /// </summary>
        public static int LinkLockWaitTime { get; set; }

        /// <summary>
        /// Maximum number of archives to allow - once exceeded, older archives will be autodeleted
        /// </summary>
        public static int MaxArchives { get; set; }

        /// <summary>
        /// Minimum amount of free space (megabytes) on storage drive - if less is available, new uploads will fail.
        /// </summary>
        public static long SpaceSafetyThreshold { get; set; }

        /// <summary>
        /// The required auth level a user needs for the current Tetrifact instance.
        /// </summary>
        public static AuthorizationLevel AuthorizationLevel { get; set; }

        /// <summary>
        /// Collection of tokens which provide write access to 
        /// </summary>
        public static IEnumerable<string> AccessTokens { get; set; }

        /// <summary>
        /// The number of transacation states to preserve.
        /// </summary>
        public static int TransactionHistoryDepth { get; set; }

        /// <summary>
        /// Diffing algorithm for binary comparison. Cannot be changed once data is created, as this will render existing data unreadable.
        /// </summary>
        public static DiffMethods DiffMethod { get; set; }

        /// <summary>
        /// Time in days redhydrated files will be kept alive after being accessed
        /// </summary>
        public static int FilePersistTimeout { get; set; }

        /// <summary>
        /// Size of chunks (in megabytes) to divide large files into. Smaller chunk sizes result in faster process times, but takes up more disk storage space.
        /// </summary>
        public static long FileChunkSize { get; set; }

        /// <summary>
        /// In minutes
        /// </summary>
        public static int AutoDiffInterval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static bool AutoClean { get; set; }

        /// <summary>
        /// In minutes.
        /// </summary>
        public static int TransactionTimeout { get; set; }

        #endregion

        #region CTORS

        static Settings()
        {
            // defaults
            AutoClean = true;
            ArchiveAvailablePollInterval = 1000;   // 1 second
            ArchiveWaitTimeout = 10 * 60;          // 10 minutes
            LinkLockWaitTime = 1000;               // 1 second
            CacheTimeout = 60 * 60;                // 1 hour
            ListPageSize = 20;
            IndexTagListLength = 20;
            PagesPerPageGroup = 20;
            MaxArchives = 10;
            FilePersistTimeout = 10;               // days
            FileChunkSize = 100 * 1000000;         // in bytes. remember, bytes * 1000000 = megabytes
            AuthorizationLevel = AuthorizationLevel.None;
            TransactionHistoryDepth = 2;
            TransactionTimeout = 60;               // minutes
            AutoDiffInterval = 1;                  // 10 minutes, in milliseconds 
            DiffMethod = DiffMethods.VcDiff;   // VcDiff is about 5 times faster than BsDiff, hence default
            ProjectsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "projects");
            LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs", "log.txt");
            TempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "temp");
            ArchivePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "archives");
            TempBinaries = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "temp_binaries");


            // try to get settings from env variables
            ListPageSize = GetSetting("LIST_PAGE_SIZE", ListPageSize);
            FileChunkSize = GetSetting("FILE_CHUNK_SIZE", FileChunkSize);
            MaxArchives = GetSetting("MAX_ARCHIVES", MaxArchives);
            AuthorizationLevel = GetSetting("AUTH_LEVEL", AuthorizationLevel);
            SpaceSafetyThreshold = GetSetting("SPACE_SAFETY_THRESHOLD", SpaceSafetyThreshold);
            TransactionHistoryDepth = GetSetting("TRANSACTION_HISTORY_DEPTH", TransactionHistoryDepth);
            ProjectsPath = GetSetting("PROJECTS_PATH", ProjectsPath);
            LogPath = GetSetting("LOG_PATH", LogPath);
            TempPath = GetSetting("TEMP_PATH", TempPath);
            ArchivePath = GetSetting("ARCHIVE_PATH", ArchivePath);
            TempBinaries = GetSetting("TEMP_BINARIES", TempBinaries);
            FilePersistTimeout = GetSetting("FILE_PERSIST_TIMEOUT", FilePersistTimeout);
            AutoDiffInterval = GetSetting("AUTO_DIFF_INTERVAL", AutoDiffInterval) * 60 * 60; // convert minute to milliseoncs;
            AutoClean = GetSetting("AUTO_CLEAN", AutoClean);
            

            // special case - access tokens can be passed in as a comma-separated string, need to split to array here
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACCESS_TOKENS"))) 
                AccessTokens = Environment.GetEnvironmentVariable("ACCESS_TOKENS").Split(",");

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DIFF_METHOD")))
                DiffMethod = (DiffMethods)Enum.Parse(typeof(DiffMethods), Environment.GetEnvironmentVariable("DIFF_METHOD").Trim());
        }

        /// <summary>
        /// Gets a string value from environment variable if that value exists. Else returns default value.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string GetSetting(string settingsName, string defaultValue)
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
        private static int GetSetting(string settingsName, int defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            if (!int.TryParse(settingsRawVariable, out defaultValue))
                throw new Exception($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid integer.");

            return defaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static bool GetSetting(string settingsName, bool defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            if (!bool.TryParse(settingsRawVariable, out defaultValue))
                throw new Exception($"Environment variable for {settingsName} ({settingsRawVariable}) is not a valid bool.");

            return defaultValue;
        }

        /// <summary>
        /// Safely gets long setting from environment variable. Logs error if value is invalid.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static long GetSetting(string settingsName, long defaultValue)
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
        private static TEnum GetSetting<TEnum>(string settingsName, TEnum defaultValue)
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
