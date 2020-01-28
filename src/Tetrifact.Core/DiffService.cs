using System.Collections.Generic;
using System.Linq;

namespace Tetrifact.Core
{
    public class DiffService
    {
        private IPackageList _packageList;

        private IPackageCreate _packageCreate;

        public DiffService(IPackageList packageList, IPackageCreate packageCreate) 
        {
            _packageList = packageList;
            _packageCreate = packageCreate;
        }

        public void ProcessAll() 
        {
            foreach (string project in _packageList.GetProjects()) 
            {
                // processed oldest first
                IEnumerable<string> undiffedPackages = _packageList.GetUndiffedPackages(project).OrderBy(r => r.CreatedUtc).Select(r => r.Id);
                foreach(string undiffedPackage in undiffedPackages)
                    _packageCreate.CreateDiffed(project, undiffedPackage);
            }
        }

    }
}
