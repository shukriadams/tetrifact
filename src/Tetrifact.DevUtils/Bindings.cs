using Ninject.Modules;
using Tetrifact.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.IO.Abstractions;

namespace Tetrifact.DevUtils
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<IPackageDiffService>().To<PackageDiffService>();
            Bind<IPackageCreateService>().To<PackageCreateService>();
            Bind<IIndexReadService>().To<IndexReadService>();
            Bind<ISettings>().To<Settings>();
            Bind<IPackageCreateWorkspace>().To<PackageCreateWorkspace>();
            Bind<IMemoryCache>().To<MemcacheShim>();
            Bind<IPackageListCache>().To<PackageListCache>();
            Bind<ILogger<IPackageCreateWorkspace>>().To<FileLogger<PackageCreateWorkspace>>();
            Bind<ILogger<ISettings>>().To<FileLogger<ISettings>>();
            Bind<ILogger<IPackageCreateService>>().To<FileLogger<IPackageCreateService>>();
            Bind<ILogger<IPackageDiffService>>().To<FileLogger<IPackageDiffService>>();
            Bind<ILogger<IIndexReadService>>().To<FileLogger<IIndexReadService>>();
            Bind<ILogger<ITagsService>>().To<FileLogger<ITagsService>>();
            Bind<IThread>().To<ThreadDefault>();
            Bind<ITagsService>().To<TagsService>();
            Bind<IFileSystem>().To<FileSystem>();
            Bind<IHashService>().To<HashService>();
            Bind<IArchiveService>().To<ArchiveService>();
        }
    }
}
