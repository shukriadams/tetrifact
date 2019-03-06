using System;
using System.IO;

namespace Tetrifact.Core
{
    public class TetriSettings : ITetriSettings
    {
        public string PackagePath { get; set; }

        public string TempPath { get; set; }

        public string RepositoryPath { get; set; }

        public string ArchivePath { get; set; }

        public string TagsPath { get; set; }

        public int ArchiveAvailablePollInterval { get; set; }

        public int ArchiveWaitTimeout { get; set; }

        public int IndexPackageListLength { get; set; }

        public TetriSettings()
        {
            // defaults
            this.ArchiveAvailablePollInterval = 1000;   // 1 second
            this.ArchiveWaitTimeout = 10 * 60;          // 10 minutes
            this.IndexPackageListLength = 100;

            // get settings from env variables
            PackagePath = Environment.GetEnvironmentVariable("PACKAGE_PATH");
            TempPath = Environment.GetEnvironmentVariable("TEMP_PATH");
            RepositoryPath = Environment.GetEnvironmentVariable("HASH_INDEX_PATH");
            ArchivePath = Environment.GetEnvironmentVariable("ARCHIVE_PATH");
            TagsPath = Environment.GetEnvironmentVariable("TAGS_PATH");

            // fall back to defaults
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
    }
}
