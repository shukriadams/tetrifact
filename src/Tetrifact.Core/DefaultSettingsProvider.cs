using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Tetrifact.Core
{
    public class DefaultSettingsProvider : ISettingsProvider
    {
        #region FIELDS

        private static Settings _settings;

        private readonly IFileSystem _fileSystem;

        #endregion

        #region CTORS

        public DefaultSettingsProvider(IFileSystem fileSystem) 
        {
            _fileSystem = fileSystem;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// load settints from app's default expected config location
        /// </summary>
        /// <returns></returns>
        public Settings Get()
        { 
            return this._Get();
        }

        /// <summary>
        /// Load app using provided YML config
        /// </summary>
        /// <returns></returns>
        public Settings Get(string ymlConfig)
        {
            return this._Get(ymlConfig);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ymlTextContent"></param>
        /// <returns></returns>
        private Settings _Get(string ymlTextContent = null)
        {
            if (_settings == null)
            {
                _settings = new Settings();

                // try to overrride defaults from environment variables
                _settings.AllowPackageDelete = this.TryGetSetting("ALLOW_PACKAGE_DELETE", _settings.AllowPackageDelete);
                _settings.AllowPackageCreate = this.TryGetSetting("ALLOW_PACKAGE_CREATE", _settings.AllowPackageCreate);
                _settings.IsStorageCompressionEnabled = this.TryGetSetting("STORAGE_COMPRESSION", _settings.IsStorageCompressionEnabled);
                _settings.Prune = this.TryGetSetting("PRUNE", _settings.Prune);

                _settings.PruneWeeklyThreshold = this.TryGetSetting("PRUNE_WEEKLY_THRESHOLD", _settings.PruneWeeklyThreshold);
                _settings.PruneWeeklyKeep = this.TryGetSetting("PRUNE_WEEKLY_KEEP", _settings.PruneWeeklyKeep);
                _settings.PruneMonthlyThreshold = this.TryGetSetting("PRUNE_MONTHLY_THRESHOLD", _settings.PruneMonthlyThreshold);
                _settings.PruneMonthlyKeep = this.TryGetSetting("PRUNE_MONTHLY_KEEP", _settings.PruneMonthlyKeep);
                _settings.PruneYearlyThreshold = this.TryGetSetting("PRUNE_YEARLY_THRESHOLD", _settings.PruneYearlyThreshold);
                _settings.PruneYearlyKeep = this.TryGetSetting("PRUNE_YEARLY_KEEP", _settings.PruneYearlyKeep);
                _settings.MetricsGenerationInterval = this.TryGetSetting("METRICS_GENERATION_INTERVAL", _settings.MetricsGenerationInterval);
                _settings.ServerName = this.TryGetSetting("SERVER_NAME", _settings.ServerName);
                _settings.WorkerThreadCount = this.TryGetSetting("WORKER_THREAD_COUNT", _settings.WorkerThreadCount);
                _settings.ListPageSize = this.TryGetSetting("LIST_PAGE_SIZE", _settings.ListPageSize);
                _settings.MaxArchives = this.TryGetSetting("MAX_ARCHIVES", _settings.MaxArchives);
                _settings.AuthorizationLevel = this.TryGetSetting("AUTH_LEVEL", _settings.AuthorizationLevel);
                _settings.SpaceSafetyThreshold = this.TryGetSetting("SPACE_SAFETY_THRESHOLD", _settings.SpaceSafetyThreshold);
                _settings.AutoCreateArchiveOnPackageCreate = this.TryGetSetting("AUTO_CREATE_ARCHIVE_ON_PACKAGE_CREATE", _settings.AutoCreateArchiveOnPackageCreate);
                _settings.PruneIgnoreTags = this.TryGetSetting("PRUNE_IGNORE_TAGS", _settings.PruneIgnoreTags);
                _settings.LogPath = this.TryGetSetting("LOG_PATH", _settings.LogPath);

                // breaks case convention, but this is what dotnet uses under the hood, so might as well use that too
                _settings.LogLevel = this.TryGetSetting("Logging__LogLevel__System", _settings.LogLevel);

                _settings.PackageDiffsPath = this.TryGetSetting("PACKAGE_DIFFS_PATH", _settings.PackageDiffsPath);
                _settings.PackagePath = this.TryGetSetting("PACKAGE_PATH", _settings.PackagePath);
                _settings.TempPath = this.TryGetSetting("TEMP_PATH", _settings.TempPath);
                _settings.RepositoryPath = this.TryGetSetting("HASH_INDEX_PATH", _settings.RepositoryPath);
                _settings.ArchivePath = this.TryGetSetting("ARCHIVE_PATH", _settings.ArchivePath);
                _settings.TagsPath = this.TryGetSetting("TAGS_PATH", _settings.TagsPath);
                _settings.MetricsPath = this.TryGetSetting("METRICS_PATH", _settings.MetricsPath);
                _settings.CleanCronMask = this.TryGetSetting("CLEAN_CRON_MASK", _settings.CleanCronMask);
                _settings.PruneCronMask = this.TryGetSetting("PRUNE_CRON_MASK", _settings.PruneCronMask);
                _settings.MetricsCronMask = this.TryGetSetting("METRICS_CRON_MASK", _settings.MetricsCronMask);
                _settings.MetricsGenerationBufferTime = this.TryGetSetting("METRICS_GENERATION_BUFFER_TIME", _settings.MetricsGenerationBufferTime);
                _settings.DEBUG_block_prune_deletes = this.TryGetSetting("DEBUG_BLOCK_PRUNE_DELETES", _settings.DEBUG_block_prune_deletes);
                _settings.WipeTempOnStart = this.TryGetSetting("WIPE_TEMP_ON_START", _settings.WipeTempOnStart);
                _settings.ArchiveCPUThreads = this.TryGetSetting("ARCHIVE_CPU_THREADS", _settings.ArchiveCPUThreads);
                _settings.SevenZipBinaryPath = this.TryGetSetting("SEVEN_ZIP_BINARY_PATH", _settings.SevenZipBinaryPath);

                string timeBracketsAllRaw = Environment.GetEnvironmentVariable("PRUNE_BRACKETS");

                if (!string.IsNullOrEmpty(timeBracketsAllRaw))
                {
                    List<PruneBracket> brackets = new List<PruneBracket>();

                    string[] timeBracketsRaw = timeBracketsAllRaw.Split(",");
                    foreach (string timeBracketRaw in timeBracketsRaw)
                    {
                        string[] items = timeBracketRaw.Trim().Split(" ");
                        if (items.Length != 2)
                            continue;

                        string intervalRaw = items[0];
                        int amount;
                        if (!int.TryParse(items[1], out amount))
                            continue;

                        Regex regex = new Regex("^(\\d*)d?");
                        Match match = regex.Match(intervalRaw);
                        if (match.Success && match.Groups.Count == 2)
                        {
                            string daysraw = match.Groups[1].Value;

                            int days;
                            if (!int.TryParse(daysraw, out days))
                                continue;

                            brackets.Add(new PruneBracket { Amount = amount, Days = days });
                        }
                    }

                    _settings.PruneBrackets = brackets;
                }

                string downloadArchiveCompressionEnvVar = Environment.GetEnvironmentVariable("DOWNLOAD_ARCHIVE_COMPRESSION");

                if (downloadArchiveCompressionEnvVar == "0")
                    _settings.DownloadArchiveCompression = CompressionLevel.NoCompression;

                if (downloadArchiveCompressionEnvVar == "1")
                    _settings.DownloadArchiveCompression = CompressionLevel.Fastest;

                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACCESS_TOKENS")))
                    _settings.AccessTokens = Environment.GetEnvironmentVariable("ACCESS_TOKENS").Split(",");

                // do the yml thing
                if (ymlTextContent == null)
                { 
                    if (_fileSystem.File.Exists(_settings.SettingsPath)) 
                    {
                        try
                        {
                            ymlTextContent = _fileSystem.File.ReadAllText(_settings.SettingsPath);
                        }
                        catch(Exception ex)
                        { 
                            throw new Exception($"Failed to load YML config from path {_settings.SettingsPath}");
                        }
                    }
                    else 
                    { 
                        Console.WriteLine("No YML config provided for settings, and YML config found on disk. Falling back to defaults for everything");
                        ymlTextContent = string.Empty;
                    }
                }

                IDeserializer deserializer = YmlHelper.GetDeserializer();
                Settings tempConfig = deserializer.Deserialize<Settings>(ymlTextContent);
            }

            return _settings;
        }

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
            if (!int.TryParse(settingsRawVariable, out attempt))
            {
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
            if (!long.TryParse(settingsRawVariable, out attempt))
            {
                Console.WriteLine($"WARNING: Environment variable for {settingsName} ({settingsRawVariable}) is not a valid long.");
                return defaultValue;
            }

            return attempt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingsName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private bool TryGetSetting(string settingsName, bool defaultValue)
        {
            string settingsRawVariable = Environment.GetEnvironmentVariable(settingsName);
            if (settingsRawVariable == null)
                return defaultValue;

            bool attempt;
            if (!bool.TryParse(settingsRawVariable, out attempt))
            {
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

            return settingsRawVariable.Split(",", StringSplitOptions.RemoveEmptyEntries);
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
