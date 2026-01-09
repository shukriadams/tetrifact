using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

namespace Tetrifact.Web
{
    public class Startup 
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container. 
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
                // SECURITY WARNING : the limit on attachment part size is removed to support large
                // builds. 
                options.MultipartBodyLengthLimit = long.MaxValue;
            });
            
            SimpleDI di = new SimpleDI();
            
            di.Register<IIndexReadService, IndexReadService>();
            di.Register<IRepositoryCleanService, RepositoryCleanService>();
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

            di.Register<ILogger<HomeController>, Logger<HomeController>>();
            di.Register<ILogger<PruneController>, Logger<PruneController>>();
            di.Register<ILogger<PackagesController>, Logger<PackagesController>>();
            di.Register<ILogger<CleanController>, Logger<CleanController>>();
            di.Register<ILogger<FilesController>, Logger<FilesController>>();
            di.Register<ILogger<ArchivesController>, Logger<ArchivesController>>();
            di.Register<ILogger<TagsController>, Logger<TagsController>>();
            di.Register<ILogger<IPackageCreateWorkspace>, Logger<IPackageCreateWorkspace>>();
            di.Register<ILogger<IMetricsService>, Logger<IMetricsService>>();
            di.Register<ILogger<ISystemCallsService>, Logger<ISystemCallsService>>();
            di.Register<ILogger<IPackageCreateService>, Logger<IPackageCreateService>>();
            di.Register<ILogger<IPackageDiffService>, Logger<IPackageDiffService>>();
            di.Register<ILogger<IPackageListService>, Logger<IPackageListService>>();
            di.Register<ILogger<ITagsService>, Logger<ITagsService>>();
            di.Register<ILogger<IArchiveService>, Logger<IArchiveService>>();
            di.Register<ILogger<IIndexReadService>, Logger<IIndexReadService>>();
            di.Register<ILogger<IPruneService>, Logger<IPruneService>>();
            di.Register<ILogger<IProcessManager>, Logger<IProcessManager>>();
            di.Register<ILogger<IDaemon>, Logger<IDaemon>>();
            di.Register<ILogger<Daemon>, Logger<Daemon>>();
            di.Register<ILogger<MetricsCron>, Logger<MetricsCron>>();
            di.Register<ILogger<PruneCron>, Logger<PruneCron>>();
            di.Register<ILogger<CleanerCron>, Logger<CleanerCron>>();
            di.Register<ILogger<ArchiveGenerator>, Logger<ArchiveGenerator>>();
            di.Register<ILogger<ProcessManagerCron>, Logger<ProcessManagerCron>>();
            di.Register<ILogger<IRepositoryCleanService>, Logger<IRepositoryCleanService>>();

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
            di.RegisterFactory<ILogger, LogFactory>(isSingleton: true);
            
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
            
            // enable async
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // enable async
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
            //services.AddControllers().ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; });
            services.AddMemoryCache();
            services.AddResponseCompression(); // enable http compression
            services.AddSingleton<IControllerActivator, ControllerProvider>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            
            // 
            services.AddScoped<ConfigurationErrors>();
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env/*, ILoggerFactory loggerFactory*/, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
            
            //loggerFactory.AddFile(settings.LogPath);

            bool isValid = settings.Validate();
            if (!isValid)
            {
                Console.WriteLine("ERROR : Server did not start properly because of configuration errors, and is now parked in error state.");
                AppState.ConfigErrors = true;
            }
            else 
            {
                Console.WriteLine("Settings :");
                Console.WriteLine($"Archive available poll interval: {settings.ArchiveAvailablePollInterval}");
                Console.WriteLine($"Archive CPU Threads: {settings.ArchiveCPUThreads}");
                Console.WriteLine($"Archive path: {settings.ArchivePath}");
                Console.WriteLine($"Archive wait timeout: {settings.ArchiveWaitTimeout}");
                Console.WriteLine($"Authorization level: {settings.AuthorizationLevel}");
                Console.WriteLine($"Auto-create archive on package create: {settings.AutoCreateArchiveOnPackageCreate}");
                Console.WriteLine($"Cache timeout: {settings.CacheTimeout}");
                Console.WriteLine($"Download archive compression: {settings.ArchiveCompression}");
                Console.WriteLine($"Index tag list length: {settings.IndexTagListLength}");
                Console.WriteLine($"Link lock wait time: {settings.LinkLockWaitTime}");
                Console.WriteLine($"List page size: {settings.ListPageSize}");
                Console.WriteLine($"Log path: {settings.LogPath}");
                Console.WriteLine($"Max archives: {settings.MaximumArchivesToKeep}");
                Console.WriteLine($"PackagePath: {settings.PackagePath}");
                Console.WriteLine($"Pages per page group: {settings.PagesPerPageGroup}");
                Console.WriteLine($"Prune brackets: {string.Join(", ", settings.PruneBrackets)}");
                Console.WriteLine($"Repository path: {settings.RepositoryPath}");
                Console.WriteLine($"Space safety threshold: {settings.SpaceSafetyThreshold}");
                Console.WriteLine($"Tags path: {settings.TagsPath}");
                Console.WriteLine($"Temp path: {settings.TempPath}");

                Console.WriteLine("Initializing indices");
                IEnumerable<IIndexReadService> indexReaders = serviceProvider.GetServices<IIndexReadService>();
                foreach (IIndexReadService indexReader in indexReaders)
                    indexReader.Initialize();

                // start daemons after index initialization
                IEnumerable<ICron> crons = di.ResolveAll<ICron>();
                Console.WriteLine($"Starting {crons.Count()} daemons : ");
                foreach (ICron cron in crons)
                {
                    cron.Start();
                    Console.WriteLine($"{cron.GetType().Name}");
                }

                Console.WriteLine("");
                Console.WriteLine($"Server startup completed in {Global.StartTimeUtc.Ago(true)}");
                Console.WriteLine("*********************************************************************");
            }
        }
    }
}
