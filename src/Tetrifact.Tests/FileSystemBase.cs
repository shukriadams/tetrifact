using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Base class for any type which requires concrete file system structures in place
    /// </summary>
    public abstract class FileSystemBase
    {
        protected ITetriSettings Settings;
        protected ILogger<IIndexReader> Logger;
        protected IIndexReader IndexReader;

        public FileSystemBase()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().Name);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            Settings = new TetriSettings(new TestLogger<TetriSettings>());
            Settings.RepositoryPath = Path.Join(testFolder, "repository");
            Settings.PackagePath = Path.Join(testFolder, "packages");
            Settings.TempPath = Path.Join(testFolder, "temp");
            Settings.ArchivePath = Path.Join(testFolder, "archives");
            Settings.TagsPath = Path.Join(testFolder, "tags");

            Logger = new TestLogger<IIndexReader>();

            IndexReader = new Core.IndexReader(Settings, Logger);
            Thread.Sleep(200);// fixes race condition when scaffolding up index between consecutive tests
            IndexReader.Initialize();
        }
    }
}
