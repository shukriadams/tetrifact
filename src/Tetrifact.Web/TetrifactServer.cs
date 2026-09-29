using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Tetrifact.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Caching.Memory;
using Tetrifact.Web.Porter_Packages.MadScience_SimpleDI;
using System.Runtime.Loader;
using System.IO;
using System.Threading;
using Tetrifact.Core.Porter_Packages.Madscience.Loggger;

namespace Tetrifact.Web
{
    /// <summary>
    /// Core startup logic for Tetrifact. Called from Program.cs
    /// </summary>
    public class TetrifactServer 
    {
        #region FIELDS

        private IList<ICron> _daemons = new List<ICron>();

        #endregion

        #region PROPERTIES

        public IConfiguration Configuration { get; }

        #endregion

        #region CTORS
        
        public TetrifactServer(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Called by runtime. Sets up ASP-level services, registers type for IOC.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine($"Configuring services ({Global.StartTimeUtc.Ago(true)})");

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<FormOptions>(options =>
            {
                // SECURITY WARNING : the limit on attachment part size is removed to support large builds. 
                options.MultipartBodyLengthLimit = long.MaxValue;
            });
            
            SimpleDI di = new SimpleDI();
            
            di.Register<IIndexReadService, IndexReadService>();
            di.Register<IRepositoryCleanService, RepositoryCleanService>();
            di.Register<IRepositoryCleanServiceFactory, RepositoryCleanServiceFactory>();
            di.Register<IPackageCreateWorkspace, PackageCreateWorkspace>();
            di.Register<ITagsService, TagsService>();
            di.Register<IPackageCreateService, PackageCreateService>();
            di.Register<IPackageListService, PackageListService>();
            di.Register<IPackageListCache, PackageListCache>();
            di.Register<IHashService, HashService>();
            di.Register<IFileSystem, FileSystem>();
            di.Register<IFile, FileWrapper>();
            di.Register<IDirectory, DirectoryWrapper>();
            di.Register<IThread, ThreadDefault>();
            di.Register<IPruneService, PruneService>();
            di.Register<IPackageDiffService, PackageDiffService>();
            di.Register<IArchiveService, ArchiveService>();
            di.Register<IMetricsService, MetricsService>();
            di.Register<ISystemCallsService, SystemCallsService>();

            di.Register<ISettingsProvider, DefaultSettingsProvider>();
            di.Register<IDaemon, Daemon>();
            di.Register<IProcessManager, ProcessManager>();
            di.Register<ITimeProvider, TimeProvider>();
            di.Register<ITetrifactMemoryCache, TetrifactMemoryCache>();
            di.Register<IFileStreamProvider, LocalFileStreamProvider>();
            di.Register<IStorageService, LocalStorageService>();
            di.Register<IPruneBracketProvider, PruneBracketProvider>();
            di.Register<IQueueHandler, QueueHandler>();
            di.RegisterSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions { }));

            // all ICron types registered here are automatically started in Configure() method below
            di.RegisterSingleton<MetricsCron, MetricsCron>();
            di.Tag<MetricsCron, ICron>();
            di.RegisterSingleton<PruneCron, PruneCron>();
            di.Tag<PruneCron, ICron>();
            di.RegisterSingleton<CleanerCron, CleanerCron>();
            di.Tag<CleanerCron, ICron>();

            di.RegisterSingleton<ArchiveGenerator, ArchiveGenerator>();
            di.Tag<ArchiveGenerator, ICron>();
            di.RegisterSingleton<ProcessManagerCron, ProcessManagerCron>();
            di.Tag<ProcessManagerCron, ICron>();

            di.Register<HomeController, HomeController>();
            di.Register<ArchivesController, ArchivesController>();
            di.Register<CleanController, CleanController>();
            di.Register<ErrorsController, ErrorsController>();
            di.Register<FilesController, FilesController>();
            di.Register<PackagesController, PackagesController>();
            di.Register<PruneController, PruneController>();
            di.Register<TagsController, TagsController>();
            di.Register<TicketsController, TicketsController>();
            
            di.RegisterFunction<ISettings>(() => {
                ISettingsProvider settingsProvider = di.Resolve<ISettingsProvider>();
                return settingsProvider.Get();
            }, isSingleton : true);
            
            di.RegisterFunction<IProcessManagerFactory>(() =>{
                return new ProcessManagerFactory(() =>
                {
                    return di.Resolve<IProcessManager>();
                });
            });
            
            // enable async for kestrel
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // enable async for IIS
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // register HTTP endpoint filters
            services.AddScoped<ReadLevel>();
            services.AddScoped<WriteLevel>();

            // prettify JSON output
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // prevent validation errors on optional form fields / querystring
            services.AddMemoryCache();
            services.AddResponseCompression(); // enable http compression
            // add our own controller provider, this lets us remove dotnet IOC entirely
            services.AddSingleton<IControllerActivator, ControllerProvider>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddScoped<ConfigurationErrors>();

            // create single instance of log for entire app
            ISettings settings = di.Resolve<ISettings>(); 
            ILoggger log = new Loggger(System.IO.Path.Join(settings.LogPath, "log-.txt"));
            di.RegisterSingleton<ILoggger>(log);

            Program.OnShutdown =()=>{
                // gracefully stop all the things running on their own threads, etc
               
                log.Dispose();

                foreach(ICron daemon in _daemons)
                    daemon.Stop();
            };
        }


        /// <summary>
        /// Called by runtime. Configures HTTP request pipeline, and start Tetrifact workers etc. Final
        /// stage of server loading, once exits server is ready to receive requests and start doing work.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // register listener for SIGKILL / SIGTERM, which we can use to gracefully shutdown worker threads etc
            IHostApplicationLifetime applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>(); 
            applicationLifetime.ApplicationStopping.Register(() => 
            { 
                Console.WriteLine("Tetrifact shutdown order received");
                Program.IsShuttingDown = true;
                if (Program.OnShutdown != null)
                    Program.OnShutdown.Invoke();
            });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");

                // The default HSTS value is 30 days. See https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // register custom error pages
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                {
                    context.Request.Path = "/error/404";
                    await next();
                }

                if (context.Response.StatusCode == 403 && !context.Response.HasStarted)
                {
                    context.Request.Path = "/error/403";
                    await next();
                }
            });


            Console.WriteLine($"Configuring middleware ({Global.StartTimeUtc.Ago(true)})");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseResponseCompression(); // http compression
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
            });

            SimpleDI di = new SimpleDI();
            ISettings settings = di.Resolve<ISettings>(); 
            ILoggger logger = di.Resolve<ILoggger>(); 

            bool isValid = settings.Validate();
            if (!isValid)
            {
                logger.Error(this, "ERROR : Server did not start properly because of configuration errors, and is now parked in error state.");
                AppState.ConfigErrors = true;
            }
            else 
            {
                // write out settings, used to confirm that values have been correctly applied etc.
                logger.Status(this, $"Archive available poll interval: {settings.ArchiveAvailablePollInterval}");
                logger.Status(this, $"Archive CPU Threads: {settings.ArchiveCPUThreads}");
                logger.Status(this, $"Archive path: {settings.ArchivePath}");
                logger.Status(this, $"Archive wait timeout: {settings.ArchiveWaitTimeout}");
                logger.Status(this, $"Authorization level: {settings.AuthorizationLevel}");
                logger.Status(this, $"Auto-create archive on package create: {settings.AutoCreateArchiveOnPackageCreate}");
                logger.Status(this, $"Cache timeout: {settings.CacheTimeout}");
                logger.Status(this, $"Clean cron mask: {settings.CleanCronMask}");
                logger.Status(this, $"Download archive compression: {settings.ArchiveCompression}");
                logger.Status(this, $"Index tag list length: {settings.IndexTagListLength}");
                logger.Status(this, $"Link lock wait time: {settings.LinkLockWaitTime}");
                logger.Status(this, $"List page size: {settings.ListPageSize}");
                logger.Status(this, $"Log path: {settings.LogPath}");
                logger.Status(this, $"Max archives: {settings.MaximumArchivesToKeep}");
                logger.Status(this, $"PackagePath: {settings.PackagePath}");
                logger.Status(this, $"Pages per page group: {settings.PagesPerPageGroup}");
                logger.Status(this, $"Prune brackets:\n{string.Join("\n", settings.PruneBrackets)}");
                logger.Status(this, $"Prune cron mask: {settings.PruneCronMask}");
                logger.Status(this, $"Repository path: {settings.RepositoryPath}");
                logger.Status(this, $"Settings path: {settings.SettingsPath}");
                logger.Status(this, $"Space safety threshold: {settings.SpaceSafetyThreshold}");
                logger.Status(this, $"Tags path: {settings.TagsPath}");
                logger.Status(this, $"Temp path: {settings.TempPath}");
                
                // initializing reader(s) will create default indices on disk
                IEnumerable<IIndexReadService> indexReaders = di.ResolveAll<IIndexReadService>();
                foreach (IIndexReadService indexReader in indexReaders)
                    indexReader.Initialize();

                // start daemons after index initialization
                IEnumerable<ICron> crons = di.ResolveAll<ICron>();
                logger.Status(this, $"{crons.Count()} daemons enabled");
                foreach (ICron cron in crons)
                {
                    cron.Start();
                    _daemons.Add(cron);
                }

                logger.Status(this, $"Server startup completed in {Global.StartTimeUtc.Ago(true)}");
                Console.WriteLine("*********************************************************************");
            }
        }

        #endregion
    }
}
