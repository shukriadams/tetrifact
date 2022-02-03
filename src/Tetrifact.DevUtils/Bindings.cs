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
            Bind<IPackageCreate>().To<PackageCreate>();
            Bind<IIndexReader>().To<IndexReader>();
            Bind<ITetriSettings>().To<TetriSettings>();
            Bind<IWorkspace>().To<Workspace>();
            Bind<IMemoryCache>().To<MemcacheShim>();
            Bind<IPackageListCache>().To<PackageListCache>();
            Bind<ILogger<IWorkspace>>().To<FileLogger<Workspace>>();
            Bind<ILogger<ITetriSettings>>().To<FileLogger<ITetriSettings>>();
            Bind<ILogger<IPackageCreate>>().To<FileLogger<IPackageCreate>>();
            Bind<ILogger<IPackageDiffService>>().To<FileLogger<IPackageDiffService>>();
            Bind<ILogger<IIndexReader>>().To<FileLogger<IIndexReader>>();
            Bind<ILogger<ITagsService>>().To<FileLogger<ITagsService>>();
            Bind<IThread>().To<ThreadDefault>();
            Bind<ITagsService>().To<TagsService>();
            Bind<IFileSystem>().To<FileSystem>();
            Bind<IHashService>().To<HashService>();
        }
    }
}
