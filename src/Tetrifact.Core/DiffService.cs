using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public class DiffService : IDiffService
    {
        private IPackageList _packageList;

        private IPackageCreate _packageCreate;

        private ILogger<DiffService> _log;

        private ISettings _settings;

        private bool _exit;

        public DiffService(IPackageList packageList, IPackageCreate packageCreate, ILogger<DiffService> log, ISettings settings) 
        {
            _settings = settings;
            _packageList = packageList;
            _packageCreate = packageCreate;
            _log = log;
        }

        public void Start() 
        {
            _exit = false;

            // call without await, as this starts an endless watch loop
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
                    foreach (string project in _packageList.GetProjects())
                    {
                        // processed oldest first
                        IEnumerable<string> undiffedPackages = _packageList.GetUndiffedPackages(project).OrderBy(r => r.CreatedUtc).Select(r => r.Id);
                        foreach (string undiffedPackage in undiffedPackages) 
                        {
                            _log.LogInformation($"Autdiffing package {undiffedPackage}");
                            _packageCreate.CreateDiffed(project, undiffedPackage);
                        }
                    }

                    if (_exit)
                        break;

                    await Task.Delay(_settings.AutoDiffInterval); // poll every 5 seconds
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Unexpected error", ex);
            }
        }

    }
}
