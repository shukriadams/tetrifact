using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests.IndexReader
{
    /// <summary>
    /// Base uses the CORE version of IndexReader (ie, the one that actuall accesses filesystem)
    /// </summary>
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
            this.Settings.MaxArchives = 3;

            this.Logger = new TestLogger<Core.IndexReader>();

            // note that indexreader here is from core, not the test shim.
            IndexReader = new Core.IndexReader(Settings, Logger);

            Thread.Sleep(200);// resolves race condition when scaffolding up index between consecutive tests
            IndexReader.Initialize();
        }
    }
}
