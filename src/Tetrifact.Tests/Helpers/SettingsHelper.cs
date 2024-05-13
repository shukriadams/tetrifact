using System;
using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class SettingsHelper
    {
        private static ISettings _currentSettingsContext;

        public static ISettings CurrentSettingsContext 
        { 
            get {  return _currentSettingsContext; } 
            set { _currentSettingsContext = value; }
        }

        public static void SetContext(Type context)
        {
            _currentSettingsContext = Get(context);
        }

        public static ISettings GetForCurrentTest()
        { 
            if (_currentSettingsContext == null)
                return Get($"random_ctx{Guid.NewGuid()}"); 

            return _currentSettingsContext;
        }

        public static ISettings Get(Type context)
        { 
            return Get(context.GetType().Name);
        }

        /// <summary>
        /// Generates settings and thereby context for a test run. Requeres a type name, as all tests are partitioned by the type they test
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ISettings Get(string context)
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, context);
            // disk-level teardown happens here, force delete the data directory for this test
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            ISettings settings = new Core.Settings
            {
                ArchiveQueuePath = Path.Join(testFolder, "archiveQueue"),
                MetricsPath = Path.Join(testFolder, "metrics"),
                LogPath = Path.Join(testFolder, "logs"),
                PackageDiffsPath = Path.Join(testFolder, "packageDiffs"),
                RepositoryPath = Path.Join(testFolder, "repository"),
                PackagePath = Path.Join(testFolder, "packages"),
                TempPath = Path.Join(testFolder, "temp"),
                ArchivePath = Path.Join(testFolder, "archives"),
                TagsPath = Path.Join(testFolder, "tags")
            };

            // force create directories, normally this is done in IndexReadService, but we cannot rely on that being called for every settings instance
            Directory.CreateDirectory(settings.ArchivePath);
            Directory.CreateDirectory(settings.ArchiveQueuePath);
            Directory.CreateDirectory(settings.PackagePath);
            Directory.CreateDirectory(settings.TempPath);
            Directory.CreateDirectory(settings.RepositoryPath);
            Directory.CreateDirectory(settings.TagsPath);
            Directory.CreateDirectory(settings.MetricsPath);
            Directory.CreateDirectory(settings.PackageDiffsPath);

            return settings;
        }
    }
}
