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
using Tetrifact.Core;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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

            // register type injections here. Note : Settings is not deliberately not here, we always want
            // to fetch an instance of settings from ISettingsProvider.
            services.AddTransient<IIndexReadService, IndexReadService>();
            services.AddTransient<IRepositoryCleanService, RepositoryCleanService>();
            services.AddTransient<IPackageCreateWorkspace, PackageCreateWorkspace>();
            services.AddTransient<ITagsService, TagsService>();
            services.AddTransient<IPackageCreateService, PackageCreateService>();
            services.AddTransient<IPackageListService, PackageListService>();
            services.AddTransient<IPackageListCache, PackageListCache>();
            services.AddTransient<IHashService, HashService>();
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddTransient<IFile, FileWrapper>();
            services.AddTransient<IDirectory, DirectoryWrapper>();
            services.AddTransient<IThread, ThreadDefault>();
            services.AddTransient<IPruneService, PruneService>();
            services.AddTransient<IPackageDiffService, PackageDiffService>();
            services.AddTransient<IArchiveService, ArchiveService>();
            services.AddTransient<IMetricsService, MetricsService>();
            services.AddTransient<ISystemCallsService, SystemCallsService>();
            services.AddSingleton<IProcessLockManager, ProcessLockManager>();
            services.AddSingleton<ISettingsProvider, DefaultSettingsProvider>();
            services.AddTransient<IDaemon, Daemon>();
            services.AddTransient<ITimeProvideer, TimeProvider>();
            services.AddTransient<ITetrifactMemoryCache, TetrifactMemoryCache>();


            // all ICron types registered here will automatically get loaded
            services.AddTransient<ICron, MetricsCron>();
            services.AddTransient<ICron, PruneCron>();
            services.AddTransient<ICron, CleanerCron>();
            services.AddTransient<ICron, ArchiveGenerator>();
            services.AddTransient<ICron, ArchiveStatusChecker>();

            // ignore error, how else are we going to get an instaace of settings from this piece of crap Microsoft IOC framework?
            ISettingsProvider settingsProvider = services.BuildServiceProvider().GetRequiredService<ISettingsProvider>();
            ISettings settings = settingsProvider.Get();
            services.AddSingleton(settings);

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // register filterws
            services.AddScoped<ReadLevel>();
            services.AddScoped<WriteLevel>();

            // prettify JSON output
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            services.AddMemoryCache();
            services.AddResponseCompression(); // http compression
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddScoped<ConfigurationErrors>();
        }


        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
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

            IServiceProvider serviceProvider = app.ApplicationServices;
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
            });

            ISettingsProvider settingsProvider = serviceProvider.GetService<ISettingsProvider>();
            ISettings settings = settingsProvider.Get();
            loggerFactory.AddFile(settings.LogPath);

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
                Console.WriteLine($"Is storage compression enabled: {settings.StorageCompressionEnabled}");
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
                Console.WriteLine("Initializing daemons");
                IEnumerable<ICron> crons = serviceProvider.GetServices<ICron>();
                foreach (ICron cron in crons)
                    cron.Start();

                Console.WriteLine($"Server configuration complete ({Global.StartTimeUtc.Ago(true)})");
                Console.WriteLine("*********************************************************************");
            }
        }
    }
}
