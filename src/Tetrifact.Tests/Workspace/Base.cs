using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base
    {
        protected IWorkspace Workspace;
        protected ITetriSettings Settings;
        protected ILogger<Core.IndexReader> Logger;

        public Base()
        {
            string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, this.GetType().Name);
            if (Directory.Exists(testFolder))
                Directory.Delete(testFolder, true);

            Directory.CreateDirectory(testFolder);
            Settings = new TetriSettings(new TestLogger<TetriSettings>());
            Settings.RepositoryPath = Path.Join(testFolder, "HashIndex");
            Settings.PackagePath = Path.Join(testFolder, "Package");
            Settings.TempPath = Path.Join(testFolder, "Temp");

            Logger = new TestLogger<Core.IndexReader>();

            IIndexReader indexReader = new Core.IndexReader(Settings, Logger);
            Thread.Sleep(200);// fixes race condition when scaffolding up index between consecutive tests
            indexReader.Initialize();

            Workspace = new Core.Workspace(Settings);
        }
    }
}
