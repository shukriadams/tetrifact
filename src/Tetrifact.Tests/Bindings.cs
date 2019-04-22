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
            Bind<IPackageService>().To<PackageService>();
            Bind<ILogger<PackagesController>>().To<TestLogger<PackagesController>>();
            Bind<ILogger<IPackageService>>().To<TestLogger<IPackageService>>();

            Bind<IIndexReader>().To<TestIndexReader>();
            Bind<IWorkspaceProvider>().To<TestWorkspaceProvider>();
        }
    }
}
