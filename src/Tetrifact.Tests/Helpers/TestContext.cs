using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using System;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using W = Tetrifact.Web;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Root of all tests. Partitions all test state, IOC instances etc in a type that can be instantiated and disposed off per test.
    /// </summary>
    public class TestContext
    {
        StandardKernel _kernel;

        ISettings _settings;

        IProcessLockManager _lockInstance;

        TestLogger<IRepositoryCleanService> _repositoryCleanServiceLog;

        public StandardKernel Kernel { get { return _kernel; } }

        public TestContext()
        {
            _kernel = new StandardKernel();

            var ProcessLockFactory = new Func<IContext, IProcessLockManager>(context =>
            {
                if (_lockInstance == null)
                {
                    ILogger<IProcessLockManager> log = this.Get<ILogger<IProcessLockManager>>();
                    _lockInstance = new ProcessLockManager(log);
                }

                return _lockInstance;
            });

            var RepositoryCleanServiceFactory = new Func<IContext, ILogger<IRepositoryCleanService>>(context =>
            {
                if (_repositoryCleanServiceLog == null)
                    _repositoryCleanServiceLog = new TestLogger<IRepositoryCleanService>();

                return _repositoryCleanServiceLog;
            });

            var SettingsFactory = new Func<IContext, ISettings>(context =>
            {
                if (_settings == null)
                {
                    string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "__testdata", Guid.NewGuid().ToString());
                    Directory.CreateDirectory(testFolder);

                    // this should be the only place in the entire test suite that we create an instance of settings.
                    ISettings settings = new Core.Settings
                    {
                        ArchiveQueuePath = Path.Join(testFolder, "archiveQueue"),
                        MetricsPath = Path.Join(testFolder, "metrics"),
                        LogPath = Path.Join(testFolder, "logs"),
                        PackageDiffsPath = Path.Join(testFolder, "packageDiffs"),
                        RepositoryPath = Path.Join(testFolder, "repository"),
                        PackagePath = Path.Join(testFolder, "packages"),
                        TempPath = Path.Join(testFolder, "temp"),
                        ArchivePath = Path.Join(testFolder, "archives"),
                        TagsPath = Path.Join(testFolder, "tags")
                    };

                    // force create directories, normally this is done in IndexReadService, but we cannot rely on that being called for every settings instance
                    Directory.CreateDirectory(settings.ArchivePath);
                    Directory.CreateDirectory(settings.ArchiveQueuePath);
                    Directory.CreateDirectory(settings.PackagePath);
                    Directory.CreateDirectory(settings.TempPath);
                    Directory.CreateDirectory(settings.RepositoryPath);
                    Directory.CreateDirectory(settings.TagsPath);
                    Directory.CreateDirectory(settings.MetricsPath);
                    Directory.CreateDirectory(settings.PackageDiffsPath);

                    _settings = settings;
                }
                return _settings;
            });

            _kernel.Bind<ISettings>().ToMethod(SettingsFactory).InSingletonScope();
            _kernel.Bind<IMemoryCache>().To<TestMemoryCache>();
            _kernel.Bind<IIndexReadService>().To<IndexReadService>();
            _kernel.Bind<IRepositoryCleanService>().To<RepositoryCleanService>();
            _kernel.Bind<IPackageListService>().To<PackageListService>();
            _kernel.Bind<IFileSystem>().To<FileSystem>();
            _kernel.Bind<IDirectory>().To<DirectoryWrapper>();
            _kernel.Bind<IFile>().To<FileWrapper>();
            _kernel.Bind<IHashService>().To<HashService>();
            _kernel.Bind<IPackageListCache>().To<PackageListCache>();
            _kernel.Bind<ITagsService>().To<Core.TagsService>();
            _kernel.Bind<IPackageCreateService>().To<PackageCreateService>();
            _kernel.Bind<IPackageCreateWorkspace>().To<PackageCreateWorkspace>();
            _kernel.Bind<IThread>().To<ThreadDefault>();
            _kernel.Bind<IPruneService>().To<PruneService>();
            _kernel.Bind<IPackageDiffService>().To<PackageDiffService>();
            _kernel.Bind<IArchiveService>().To<Core.ArchiveService>();
            _kernel.Bind<IProcessLockManager>().ToMethod(ProcessLockFactory).InSingletonScope(); // ordinary singleton binding not working, use factory instead
            _kernel.Bind<IMetricsService>().To<MetricsService>();
            _kernel.Bind<ISystemCallsService>().To<SystemCallsService>();
            _kernel.Bind<IHostApplicationLifetime>().To<TestHostApplicationLifetime>();
            _kernel.Bind<W.IDaemon>().To<TestDaemon>();
            _kernel.Bind<ITimeProvideer>().To<TimeProvider>();
            _kernel.Bind<ITetrifactMemoryCache>().To<TetrifactTestMemoryCache>();

            _kernel.Bind<ILogger<W.HomeController>>().To<TestLogger<W.HomeController>>();
            _kernel.Bind<ILogger<W.PruneController>>().To<TestLogger<W.PruneController>>();
            _kernel.Bind<ILogger<W.PackagesController>>().To<TestLogger<W.PackagesController>>();
            _kernel.Bind<ILogger<W.CleanController>>().To<TestLogger<W.CleanController>>();
            _kernel.Bind<ILogger<W.FilesController>>().To<TestLogger<W.FilesController>>();
            _kernel.Bind<ILogger<W.ArchivesController>>().To<TestLogger<W.ArchivesController>>();
            _kernel.Bind<ILogger<W.TagsController>>().To<TestLogger<W.TagsController>>();
            _kernel.Bind<ILogger<IPackageCreateWorkspace>>().To<TestLogger<IPackageCreateWorkspace>>();
            _kernel.Bind<ILogger<IMetricsService>>().To<TestLogger<IMetricsService>>();
            _kernel.Bind<ILogger<ISystemCallsService>>().To<TestLogger<ISystemCallsService>>();
            _kernel.Bind<ILogger<IPackageCreateService>>().To<TestLogger<IPackageCreateService>>();
            _kernel.Bind<ILogger<IPackageDiffService>>().To<TestLogger<IPackageDiffService>>();
            _kernel.Bind<ILogger<ITagsService>>().To<TestLogger<ITagsService>>();
            _kernel.Bind<ILogger<IArchiveService>>().To<TestLogger<IArchiveService>>();
            _kernel.Bind<ILogger<IPackageListService>>().To<TestLogger<IPackageListService>>();
            //Bind<ILogger<IRepositoryCleanService>>().To<TestLogger<IRepositoryCleanService>>();
            _kernel.Bind<ILogger<IIndexReadService>>().To<TestLogger<IIndexReadService>>();
            _kernel.Bind<ILogger<IPruneService>>().To<TestLogger<IPruneService>>();
            _kernel.Bind<ILogger<IProcessLockManager>>().To<TestLogger<IProcessLockManager>>();
            _kernel.Bind<ILogger<W.IDaemon>>().To<TestLogger<W.IDaemon>>();
            _kernel.Bind<ILogger<W.IDaemon>>().To<TestLogger<W.IDaemon>>();

            _kernel.Bind<ILogger<IRepositoryCleanService>>().ToMethod(RepositoryCleanServiceFactory).InSingletonScope();
        }


        private T Get<T>(ConstructorArgument[] args)
        {
            return _kernel.Get<T>(args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            return Get<T>(new ConstructorArgument[] { });
        }

        /// <summary>
        /// Creates an instance with a single constructor argument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public T Get<T>(string name, object arg)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name, arg)
            });
        }

        /// <summary>
        /// Creates an instance with two constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg1Name"></param>
        /// <param name="arg1Value"></param>
        /// <param name="arg2Name"></param>
        /// <param name="arg2Value"></param>
        /// <returns></returns>
        public T Get<T>(string arg1Name, object arg1Value, string arg2Name, object arg2Value)
        {
            return Get<T>(new[] {
                new ConstructorArgument(arg1Name, arg1Value),
                new ConstructorArgument(arg2Name, arg2Value)
            });
        }

        /// <summary>
        /// Creates an instance with three constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <param name="name2"></param>
        /// <param name="arg2"></param>
        /// <param name="name3"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        public T Get<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3)
            });
        }

        /// <summary>
        /// Creates an instance with four constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <param name="name2"></param>
        /// <param name="arg2"></param>
        /// <param name="name3"></param>
        /// <param name="arg3"></param>
        /// <param name="name4"></param>
        /// <param name="arg4"></param>
        /// <returns></returns>
        public T Get<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3, string name4, object arg4)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3),
                new ConstructorArgument(name4, arg4)
            });
        }

        /// <summary>
        /// Creates an instance with five constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <param name="name2"></param>
        /// <param name="arg2"></param>
        /// <param name="name3"></param>
        /// <param name="arg3"></param>
        /// <param name="name4"></param>
        /// <param name="arg4"></param>
        /// <param name="name5"></param>
        /// <param name="arg5"></param>
        /// <returns></returns>
        public T Get<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3, string name4, object arg4, string name5, object arg5)
        {
            return Get<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3),
                new ConstructorArgument(name4, arg4),
                new ConstructorArgument(name5, arg5)
            });
        }
    }
}
