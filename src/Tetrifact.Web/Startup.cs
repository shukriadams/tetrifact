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
using System.IO;
using System.IO.Abstractions;
using Tetrifact.Core;

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

            // register type injections here
            services.AddTransient<ISettings, Settings>();
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
            services.AddTransient<Daemon, Daemon>();
            services.AddTransient<IThread, ThreadDefault>();
            services.AddTransient<IPackagePruneService, PackagePruneService>();
            services.AddTransient<IPackageDiffService, PackageDiffService>();
            services.AddTransient<IArchiveService, ArchiveService>();

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
            services.AddResponseCompression();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
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


            string logPath = Environment.GetEnvironmentVariable("LOG_PATH");
            if (string.IsNullOrEmpty(logPath))
                logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "logs", "log.txt");

            loggerFactory.AddFile(logPath);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseResponseCompression();

            IServiceProvider serviceProvider = app.ApplicationServices;
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
            });
            int daemonInterval = 1000 * 60 * 10; // 60000 = 10 minutes

            ISettings settings = serviceProvider.GetService<ISettings>();
            Console.WriteLine("*********************************************************************");
            Console.WriteLine("TETRIFACT SERVER starting");
            Console.WriteLine("");
            Console.WriteLine("Settings:");
            Console.WriteLine($"Archive available poll interval: {settings.ArchiveAvailablePollInterval}");
            Console.WriteLine($"Archive path: {settings.ArchivePath}");
            Console.WriteLine($"Archive wait timeout: {settings.ArchiveWaitTimeout}");
            Console.WriteLine($"Authorization level: {settings.AuthorizationLevel}");
            Console.WriteLine($"Autocreate archive on package create: {settings.AutoCreateArchiveOnPackageCreate}");
            Console.WriteLine($"Cache timeout: {settings.CacheTimeout}");
            Console.WriteLine($"Daemon interval: {daemonInterval}");
            Console.WriteLine($"Download archive compression: {settings.DownloadArchiveCompression}");
            Console.WriteLine($"Index tag list length: {settings.IndexTagListLength}");
            Console.WriteLine($"Is storage compression enabled: {settings.IsStorageCompressionEnabled}");
            Console.WriteLine($"Link lock wait time: {settings.LinkLockWaitTime}");
            Console.WriteLine($"List page size: {settings.ListPageSize}");
            Console.WriteLine($"Log path: {logPath}");
            Console.WriteLine($"Max archives: {settings.MaxArchives}");
            Console.WriteLine($"PackagePath: {settings.PackagePath}");
            Console.WriteLine($"Pages per page group: {settings.PagesPerPageGroup}");
            Console.WriteLine($"Repository path: {settings.RepositoryPath}");
            Console.WriteLine($"Space safety threshold: {settings.SpaceSafetyThreshold}");
            Console.WriteLine($"Tags path: {settings.TagsPath}");
            Console.WriteLine($"Temp path: {settings.TempPath}");
            Console.WriteLine("*********************************************************************");

            // initialize indexes
            IEnumerable<IIndexReadService> indexReaders = serviceProvider.GetServices<IIndexReadService>();
            foreach (IIndexReadService indexReader in indexReaders)
                indexReader.Initialize();
            Console.WriteLine("Indexes initialized");

            // start daemon
            Daemon daemon = serviceProvider.GetService<Daemon>();
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DAEMON_INTERVAL")))
                int.TryParse(Environment.GetEnvironmentVariable("DAEMON_INTERVAL"), out daemonInterval);

            daemon.Start(daemonInterval);
            Console.WriteLine("Daemon started");
        }
    }
}
