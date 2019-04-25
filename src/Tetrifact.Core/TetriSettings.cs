﻿using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Tetrifact.Core
{
    public class TetriSettings : ITetriSettings
    {
        #region FIELDS

        private ILogger<ITetriSettings> _log;

        #endregion

        #region PROPERTIES

        public string PackagePath { get; set; }

        public string TempPath { get; set; }

        public string RepositoryPath { get; set; }

        public string ArchivePath { get; set; }

        public string TagsPath { get; set; }

        public int ArchiveAvailablePollInterval { get; set; }

        public int ArchiveWaitTimeout { get; set; }

        public int ListPageSize { get; set; }

        public int IndexTagListLength { get; set; }

        public int PagesPerPageGroup { get; set; }

        public int CacheTimeout { get; set; }

        public int MaxArchives { get; set; }

        #endregion

        #region CTORS

        public TetriSettings(ILogger<ITetriSettings> log)
        {
            _log = log;

            // defaults
            this.ArchiveAvailablePollInterval = 1000;   // 1 second
            this.ArchiveWaitTimeout = 10 * 60;          // 10 minutes
            this.ListPageSize = 50;
            this.IndexTagListLength = 20;
            this.PagesPerPageGroup = 20;
            this.CacheTimeout = 60 * 60;                // 1 hour
            this.MaxArchives = 10;

            // get settings from env variables
            PackagePath = Environment.GetEnvironmentVariable("PACKAGE_PATH");
            TempPath = Environment.GetEnvironmentVariable("TEMP_PATH");
            RepositoryPath = Environment.GetEnvironmentVariable("HASH_INDEX_PATH");
            ArchivePath = Environment.GetEnvironmentVariable("ARCHIVE_PATH");
            TagsPath = Environment.GetEnvironmentVariable("TAGS_PATH");

            if (Environment.GetEnvironmentVariable("LIST_PAGE_SIZE") != null)
            {
                int listPageSize = this.ListPageSize;
                if (Int32.TryParse(Environment.GetEnvironmentVariable("LIST_PAGE_SIZE"), out listPageSize))
                    this.ListPageSize = listPageSize;
                else
                    _log.LogError($"Environment variable for LIST_PAGE_SIZE ({Environment.GetEnvironmentVariable("LIST_PAGE_SIZE")}) is not a valid integer.");
            }

            if (Environment.GetEnvironmentVariable("MAX_ARCHIVES") != null)
            {
                int maxArchives = this.MaxArchives;
                if (Int32.TryParse(Environment.GetEnvironmentVariable("MAX_ARCHIVES"), out maxArchives))
                    this.MaxArchives = maxArchives;
                else
                    _log.LogError($"Environment variable for MAX_ARCHIVES ({Environment.GetEnvironmentVariable("MAX_ARCHIVES")}) is not a valid integer.");
            }


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

        #endregion
    }
}
