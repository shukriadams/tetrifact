using Microsoft.Extensions.Caching.Memory;
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
            Bind<ILogger<PackagesController>>().To<TestLogger<PackagesController>>();
            Bind<ILogger<IPackageCreate>>().To<TestLogger<IPackageCreate>>();
            Bind<ILogger<ITetriSettings>>().To<TestLogger<ITetriSettings>>();
            Bind<IIndexReader>().To<TestIndexReader>();
            Bind<IRepositoryCleaner>().To<TestRepositoryCleaner>();
            Bind<IWorkspace>().To<TestingWorkspace>();
            Bind<IPackageList>().To<TestPackageList>();

            Bind<ITagsService>().To<Core.TagsService>();
            Bind<IPackageCreate>().To<Core.PackageCreate>();
            Bind<ILogger<FilesController>>().To<TestLogger<FilesController>>();
            Bind<ILogger<ArchivesController>>().To<TestLogger<ArchivesController>>();
            Bind<ILogger<TagsController>>().To<TestLogger<TagsController>>();
            Bind<ILogger<ITagsService>>().To<TestLogger<ITagsService>>();
        }
    }
}
