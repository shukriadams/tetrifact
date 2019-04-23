using Ninject.Modules;
using Tetrifact.Core;
using Microsoft.Extensions.Logging;

namespace Tetrifact.DevUtils
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IPackageService>().To<PackageService>();
            Bind<IIndexReader>().To<IndexReader>();
            Bind<ITetriSettings>().To<TetriSettings>();
            Bind<IWorkspaceProvider>().To<WorkspaceProvider>();
            Bind<ILogger<ITetriSettings>>().To<FileLogger<ITetriSettings>>();
            Bind<ILogger<IPackageService>>().To<FileLogger<IPackageService>>();
            Bind<ILogger<IIndexReader>>().To<FileLogger<IIndexReader>>();
        }
    }
}
