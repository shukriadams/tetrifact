using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using System;
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;
using Tetrifact.Web;
using Tetrifact.Core.Porter_Packages.Madscience.Loggger;
using W = Tetrifact.Web;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Root of all tests. Partitions all test state, IOC instances etc in a type that can be instantiated and disposed off per test.
    /// </summary>
    public class TestContext
    {
        #region FIELDS
        
        private StandardKernel _kernel;

        private ISettings _settings;

        private TestLogger _repositoryCleanServiceLog;

        #endregion
        
        #region PROPERTIES
        
        public StandardKernel Kernel { get { return _kernel; } }

        #endregion
        
        #region CTORS
        
        public TestContext()
        {
            _kernel = new StandardKernel();
            
            var SettingsFactory = new Func<IContext, ISettings>(context =>
            {
                if (_settings == null)
                {
                    string testFolder = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "__testdata", Guid.NewGuid().ToString());
                    Directory.CreateDirectory(testFolder);

                    // this should be the only place in the entire test suite that we create an instance of settings.
                    ISettings settings = new Settings
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

            _kernel.Bind<ILoggger>().To<TestLogger>();
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
            _kernel.Bind<IProcessManager>().To<Core.ProcessManager>();
            
            _kernel.Bind<IProcessManagerFactory>().ToConstant(new ProcessManagerFactory(() => {
                return _kernel.Get<IProcessManager>();
            }));

            _kernel.Bind<IPruneServiceFactory>().ToConstant(new PruneServiceFactory(() => {
                return _kernel.Get<IPruneService>();
            }));

            _kernel.Bind<IRepositoryCleanServiceFactory>().ToConstant(new RepositoryCleanServiceFactory(() => {
                return _kernel.Get<IRepositoryCleanService>();
            }));

            _kernel.Bind<IMetricsService>().To<MetricsService>();
            _kernel.Bind<ISystemCallsService>().To<SystemCallsService>();
            _kernel.Bind<IHostApplicationLifetime>().To<TestHostApplicationLifetime>();
            _kernel.Bind<W.IDaemon>().To<TestDaemon>();
            _kernel.Bind<ITimeProvider>().To<TimeProvider>();
            _kernel.Bind<ITetrifactMemoryCache>().To<TetrifactTestMemoryCache>();
            _kernel.Bind<IFileStreamProvider>().To<LocalFileStreamProvider>();
            _kernel.Bind<IStorageService>().To<LocalStorageService>();
            _kernel.Bind<IPruneBracketProvider>().To<PruneBracketProvider>();
            _kernel.Bind<IQueueHandler>().To<QueueHandler>();
            
            // force wipe memcache at start of each test
            IMemoryCache memCach = _kernel.Get<IMemoryCache>();
            memCach.Dispose();
        }

        #endregion

        private T Instantiate<T>(ConstructorArgument[] args)
        {
            return _kernel.Get<T>(args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Instantiate<T>()
        {
            return Instantiate<T>(new ConstructorArgument[] { });
        }

        /// <summary>
        /// Creates an instance with a single constructor argument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name1"></param>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public T Instantiate<T>(string name, object arg)
        {
            return Instantiate<T>(new[] {
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
        public T Instantiate<T>(string arg1Name, object arg1Value, string arg2Name, object arg2Value)
        {
            return Instantiate<T>(new[] {
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
        public T Instantiate<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3)
        {
            return Instantiate<T>(new[] {
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
        public T Instantiate<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3, string name4, object arg4)
        {
            return Instantiate<T>(new[] {
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
        public T Instantiate<T>(string name1, object arg1, string name2, object arg2, string name3, object arg3, string name4, object arg4, string name5, object arg5)
        {
            return Instantiate<T>(new[] {
                new ConstructorArgument(name1, arg1),
                new ConstructorArgument(name2, arg2),
                new ConstructorArgument(name3, arg3),
                new ConstructorArgument(name4, arg4),
                new ConstructorArgument(name5, arg5)
            });
        }
    }
}
