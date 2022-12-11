using System;
using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class SettingsHelper
    {
        /// <summary>
        /// Generates settings and thereby context for a test run. Requeres a type name, as all tests are partitioned by the type they test
        /// </summary>
        /// <param name="testTypeContext"></param>
        /// <returns></returns>
        public static ISettings GetSettings(string testTypeContext)
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, testTypeContext);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            return new Core.Settings
            {
                RepositoryPath = Path.Join(testFolder, "repository"),
                PackagePath = Path.Join(testFolder, "packages"),
                TempPath = Path.Join(testFolder, "temp"),
                ArchivePath = Path.Join(testFolder, "archives"),
                TagsPath = Path.Join(testFolder, "tags")
            };
        }
    }
}
