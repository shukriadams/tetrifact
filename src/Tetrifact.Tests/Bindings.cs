using Microsoft.Extensions.Logging;
using Ninject.Modules;
using Tetrifact.Core;
using Tetrifact.Web;

namespace Tetrifact.Tests
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ITetriSettings>().To<TetriSettings>();
            Bind<IPackageCreate>().To<PackageCreate>();
            Bind<ILogger<PackagesController>>().To<TestLogger<PackagesController>>();
            Bind<ILogger<IPackageCreate>>().To<TestLogger<IPackageCreate>>();
            Bind<ILogger<ITetriSettings>>().To<TestLogger<ITetriSettings>>();
            Bind<IIndexReader>().To<TestIndexReader>();
            Bind<IWorkspace>().To<TestingWorkspace>();
            Bind<IPackageList>().To<TestPackageList>();
        }
    }
}
