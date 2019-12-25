using Microsoft.Extensions.Logging;
using Ninject.Modules;
using Tetrifact.Core;
using Tetrifact.Web;

namespace Tetrifact.Tests
{
    /// <summary>
    /// IOC binding needs to be done here - in Tetrifact.Web we use ASP.Net's IOC system, but for testing we rely on Ninject.
    /// </summary>
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ITetriSettings>().To<TetriSettings>();
            Bind<IIndexReader>().To<TestIndexReader>();
            Bind<ICleaner>().To<TestRepositoryCleaner>();
            Bind<IPackageList>().To<TestPackageList>();
            Bind<IApplicationLogic>().To<AppLogic>();
            Bind<IProjectService>().To<Core.ProjectService>();

            Bind<ITagsService>().To<Core.TagsService>();
            Bind<IPackageCreate>().To<Core.PackageCreate>();
            Bind<IPackageDeleter>().To<Core.PackageDeleter>();
            Bind<ILogger<PackagesController>>().To<TestLogger<PackagesController>>();
            Bind<ILogger<IPackageCreate>>().To<TestLogger<IPackageCreate>>();
            Bind<ILogger<IProjectService>>().To<TestLogger<IProjectService>>();
            Bind<ILogger<ProjectsController>>().To<TestLogger<ProjectsController>>();
            Bind<ILogger<FilesController>>().To<TestLogger<FilesController>>();
            Bind<ILogger<ArchivesController>>().To<TestLogger<ArchivesController>>();
            Bind<ILogger<TagsController>>().To<TestLogger<TagsController>>();
            Bind<ILogger<ITagsService>>().To<TestLogger<ITagsService>>();
            Bind<ILogger<IPackageDeleter>>().To<TestLogger<IPackageDeleter>>();
        }
    }
}
