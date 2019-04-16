using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests.IndexReader
{
    public abstract class Base
    {
        protected IIndexReader IndexReader;
        protected ITetriSettings Settings;
        protected ILogger<Core.IndexReader> Logger;

        public Base()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().Name);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);

            this.Settings = new TetriSettings(new TestLogger<TetriSettings>());
            this.Settings.RepositoryPath = Path.Join(testFolder, "HashIndex");
            this.Settings.PackagePath = Path.Join(testFolder, "Package");
            this.Settings.TempPath = Path.Join(testFolder, "Temp");
            this.Settings.ArchivePath = Path.Join(testFolder, "Archives");

            this.Logger = new TestLogger<Core.IndexReader>();

            IndexReader = new Core.IndexReader(Settings, Logger);
            Thread.Sleep(200);// fixes race condition when scaffolding up index between consecutive tests
            IndexReader.Initialize();
        }
    }
}
