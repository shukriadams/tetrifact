using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Ninject.Modules;
using System.IO.Abstractions;
using Tetrifact.Core;
using Tetrifact.Web;

namespace Tetrifact.Tests
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ISettings>().To<Settings>();
            Bind<IMemoryCache>().To<TestMemoryCache>();
            Bind<IIndexReadService>().To<TestIndexReader>();
            Bind<IRepositoryCleanService>().To<TestRepositoryCleaner>();
            Bind<IPackageListService>().To<PackageListService>();
            Bind<IFileSystem>().To<FileSystem>();
            Bind<IHashService>().To<HashService>();
            Bind<IPackageListCache>().To<TestPackageListCache>();
            Bind<ITagsService>().To<Core.TagsService>();
            Bind<IPackageCreateService>().To<PackageCreateService>();
            Bind<IPackageCreateWorkspace>().To<PackageCreateWorkspace>();
            Bind<IThread>().To<ThreadDefault>();
            Bind<IPackagePruneService>().To<PackagePruneService>();
            Bind<IPackageDiffService>().To<PackageDiffService>();
            Bind<IArchiveService>().To<Core.ArchiveService>();
            Bind<ILogger<PackagesController>>().To<TestLogger<PackagesController>>();
            Bind<ILogger<CleanController>>().To<TestLogger<CleanController>>();
            Bind<ILogger<FilesController>>().To<TestLogger<FilesController>>();
            Bind<ILogger<ArchivesController>>().To<TestLogger<ArchivesController>>();
            Bind<ILogger<TagsController>>().To<TestLogger<TagsController>>();
            Bind<ILogger<IPackageCreateWorkspace>>().To<TestLogger<IPackageCreateWorkspace>>();
            Bind<ILogger<IPackageCreateService>>().To<TestLogger<IPackageCreateService>>();
            Bind<ILogger<ISettings>>().To<TestLogger<ISettings>>();
            Bind<ILogger<IPackageDiffService>>().To<TestLogger<IPackageDiffService>>();
            Bind<ILogger<ITagsService>>().To<TestLogger<ITagsService>>();
            Bind<ILogger<IArchiveService>>().To<TestLogger<IArchiveService>>();
            Bind<ILogger<IPackageListService>>().To<TestLogger<IPackageListService>>();
        }
    }
}
