using System;
using System.IO;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class SettingsHelper
    {
        private static ISettings _currentSettingsContext;
        private static object _threadLock = new object();
        public static ISettings CurrentSettingsContext 
        { 
            get {  return _currentSettingsContext; } 
        }

        public static void SetContext(Type context)
        {
            lock (_threadLock)
                _currentSettingsContext = Get(context.Name);
        }

        /// <summary>
        /// Generates settings and thereby context for a test run. Requeres a type name, as all tests are partitioned by the type they test
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ISettings Get(string context)
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "__testdata", context);
            
            // disk-level teardown happens here, force delete the data directory for this test
            int tries = 0;
            while (tries < 100)
            {
                try
                {
                    if (Directory.Exists(testFolder))
                        Directory.Delete(testFolder, true);

                    break;
                }
                catch
                {
                    tries++;
                    Thread.Sleep(100);
                }
            }

            if (tries == 100)
                throw new Exception($"failed to delete test folder {testFolder}");

            Directory.CreateDirectory(testFolder);

            // this should be the only place in the entire test suite that we create an instance of settings.
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
