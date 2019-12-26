using Ninject.Modules;
using Tetrifact.Core;
using Microsoft.Extensions.Logging;
using Tetrifact.Dev;

namespace Tetrifact.DevUtils
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IPackageCreate>().To<PackageCreate>();
            Bind<IPackageDeleter>().To<PackageDeleter>();
            Bind<IIndexReader>().To<IndexReader>();
            Bind<ISettings>().To<Settings>();
            Bind<ILogger<ISettings>>().To<FileLogger<ISettings>>();
            Bind<ILogger<IPackageCreate>>().To<FileLogger<IPackageCreate>>();
            Bind<ILogger<IIndexReader>>().To<FileLogger<IIndexReader>>();
        }
    }
}
