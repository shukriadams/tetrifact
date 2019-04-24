using Ninject.Modules;
using Tetrifact.Core;
using Microsoft.Extensions.Logging;

namespace Tetrifact.DevUtils
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IPackageCreate>().To<PackageCreate>();
            Bind<IIndexReader>().To<IndexReader>();
            Bind<ITetriSettings>().To<TetriSettings>();
            Bind<IWorkspace>().To<Workspace>();
            Bind<ILogger<ITetriSettings>>().To<FileLogger<ITetriSettings>>();
            Bind<ILogger<IPackageCreate>>().To<FileLogger<IPackageCreate>>();
            Bind<ILogger<IIndexReader>>().To<FileLogger<IIndexReader>>();
        }
    }
}
