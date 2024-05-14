/*
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ninject.Activation;
using Ninject.Modules;
using System;
using System.IO.Abstractions;
using Tetrifact.Core;
using W=Tetrifact.Web;

namespace Tetrifact.Tests
{
    public class Bindings : NinjectModule
    {
        public static Func<IContext, ISettings> SettingsFactory = new Func<IContext, ISettings>(context =>
        {
            return Settings;
        });

        private static ILock _lockInstance;
        public static Func<IContext, ILock> ProcessLockFactory = new Func<IContext, ILock>(context =>
        {
            if (_lockInstance == null)
            {
                ILogger<ILock> log = NinjectHelper.Get<ILogger<ILock>>();
                _lockInstance = new ProcessLock(log);
            }
            
            return _lockInstance;
        });

        public static TestLogger<IRepositoryCleanService> RepositoryCleanServiceLog;

        public static Func<IContext, ILogger<IRepositoryCleanService>> RepositoryCleanServiceFactory = new Func<IContext, ILogger<IRepositoryCleanService>>(context =>
        {
            if (RepositoryCleanServiceLog == null)
                RepositoryCleanServiceLog = new TestLogger<IRepositoryCleanService>();

            return RepositoryCleanServiceLog;
        });


        public override void Load()
        {

            Bind<ISettings>().ToMethod(SettingsFactory).InSingletonScope();
            Bind<IMemoryCache>().To<TestMemoryCache>();
            Bind<IIndexReadService>().To<IndexReadService>();
            Bind<IRepositoryCleanService>().To<RepositoryCleanService>();
            Bind<IPackageListService>().To<PackageListService>();
            Bind<IFileSystem>().To<FileSystem>();
            Bind<IDirectory>().To<DirectoryWrapper>();
            Bind<IFile>().To<FileWrapper>();
            Bind<IHashService>().To<HashService>();
            Bind<IPackageListCache>().To<PackageListCache>();
            Bind<ITagsService>().To<Core.TagsService>();
            Bind<IPackageCreateService>().To<PackageCreateService>();
            Bind<IPackageCreateWorkspace>().To<PackageCreateWorkspace>();
            Bind<IThread>().To<ThreadDefault>();
            Bind<IPackagePruneService>().To<PackagePruneService>();
            Bind<IPackageDiffService>().To<PackageDiffService>();
            Bind<IArchiveService>().To<Core.ArchiveService>();
            Bind<ILock>().ToMethod(ProcessLockFactory).InSingletonScope(); // ordinary singleton binding not working, use factory instead
            Bind<IMetricsService>().To<MetricsService>();
            Bind<ISystemCallsService>().To<SystemCallsService>();
            Bind<IHostApplicationLifetime>().To<TestHostApplicationLifetime>();
            Bind<W.IDaemon>().To<TestDaemon>();
            Bind<ITimeProvideer>().To<TimeProvider>();

            Bind<ILogger<W.HomeController>>().To<TestLogger<W.HomeController>>();
            Bind<ILogger<W.PruneController>>().To<TestLogger<W.PruneController>>();
            Bind<ILogger<W.PackagesController>>().To<TestLogger<W.PackagesController>>();
            Bind<ILogger<W.CleanController>>().To<TestLogger<W.CleanController>>();
            Bind<ILogger<W.FilesController>>().To<TestLogger<W.FilesController>>();
            Bind<ILogger<W.ArchivesController>>().To<TestLogger<W.ArchivesController>>();
            Bind<ILogger<W.TagsController>>().To<TestLogger<W.TagsController>>();
            Bind<ILogger<IPackageCreateWorkspace>>().To<TestLogger<IPackageCreateWorkspace>>();
            Bind<ILogger<IMetricsService>>().To<TestLogger<IMetricsService>>();
            Bind<ILogger<ISystemCallsService>>().To<TestLogger<ISystemCallsService>>();
            Bind<ILogger<IPackageCreateService>>().To<TestLogger<IPackageCreateService>>();
            Bind<ILogger<IPackageDiffService>>().To<TestLogger<IPackageDiffService>>();
            Bind<ILogger<ITagsService>>().To<TestLogger<ITagsService>>();
            Bind<ILogger<IArchiveService>>().To<TestLogger<IArchiveService>>();
            Bind<ILogger<IPackageListService>>().To<TestLogger<IPackageListService>>();
            //Bind<ILogger<IRepositoryCleanService>>().To<TestLogger<IRepositoryCleanService>>();
            Bind<ILogger<IIndexReadService>>().To<TestLogger<IIndexReadService>>();
            Bind<ILogger<IPackagePruneService>>().To<TestLogger<IPackagePruneService>>();
            Bind<ILogger<ILock>>().To<TestLogger<ILock>>();
            Bind<ILogger<W.IDaemon>>().To<TestLogger<W.IDaemon>>();
            Bind<ILogger<W.IDaemon>>().To<TestLogger<W.IDaemon>>();

            Bind<ILogger<IRepositoryCleanService>>().ToMethod(RepositoryCleanServiceFactory).InSingletonScope();
        }
    }
}
*/