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
            Bind<PackageService>().To<PackageService>();
            Bind<ILogger<PackagesController>>().To<TestLogger<PackagesController>>();
            Bind<ILogger<PackageService>>().To<TestLogger<PackageService>>();

            Bind<IIndexReader>().To<TestIndexReader>();
            Bind<IWorkspaceProvider>().To<TestWorkspaceProvider>();

            
        }
    }
}
