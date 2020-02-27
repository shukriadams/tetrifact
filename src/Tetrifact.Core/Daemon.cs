using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public class Daemon : IDiffService
    {
        private IPackageList _packageList;

        private IPackageCreate _packageCreate;

        private ILogger<Daemon> _log;

        private bool _exit;

        private ICleaner _cleaner;

        public DateTime? LastRun { get; private set; }

        public Daemon(IPackageList packageList, IPackageCreate packageCreate, ILogger<Daemon> log, ICleaner cleaner) 
        {
            _packageList = packageList;
            _packageCreate = packageCreate;
            _log = log;
            _cleaner = cleaner;
        }

        public void Start() 
        {
            _exit = false;

            // call without await, this starts an endless loop
            Task.Run(async () => this.ProcessAll());
        }

        public void Stop() 
        {
            _exit = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ProcessAll()
        {
            try
            {
                while (true)
                {
                    this.LastRun = DateTime.UtcNow;

                    this.Diff();

                    this.Clean();

                    if (_exit)
                        break;

                    await Task.Delay(Settings.AutoDiffInterval);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Unexpected error : {ex}", ex);
            }
        }

        private void Clean() 
        {
            if (!Settings.AutoClean)
                return;

            foreach (string project in _packageList.GetProjects())
                if (Settings.AutoClean)
                    _cleaner.Clean(project);
        }

        private void Diff() 
        {
            foreach (string project in _packageList.GetProjects())
            {
                // processed oldest first
                IEnumerable<string> undiffedPackages = _packageList.GetUndiffedPackages(project).OrderBy(r => r.CreatedUtc).Select(r => r.Name);
                foreach (string undiffedPackage in undiffedPackages)
                {
                    _log.LogInformation($"Autdiffing package {undiffedPackage}");
                    try
                    {
                        _packageCreate.CreateDiffed(project, undiffedPackage);
                    }
                    catch (Exception ex) 
                    {
                        _packageCreate.MarkFailedDiff(project, undiffedPackage);
                        throw ex;
                    }
                }
            }
        }
    }
}
