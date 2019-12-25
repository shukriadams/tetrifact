using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public class Startup
    {

        /// <summary>
        /// Add services to the container. 
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
            services.AddTransient<ITetriSettings, TetriSettings>();
            services.AddTransient<IRepositoryCleaner, Cleaner>();
            services.AddTransient<ITagsService, TagsService>();
            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IPackageCreate, PackageCreate>();
            services.AddTransient<IPackageList, PackageList>();
            services.AddTransient<IAppLogic, AppLogic>();
            services.AddTransient<IIndexReader, IndexReader>();
            services.AddTransient<IPackageDeleter, PackageDeleter>();

            // register filterws
            services.AddScoped<ReadLevel>();
            services.AddScoped<WriteLevel>();

            // prettify JSON output
            services.AddMvc(
                    option => option.EnableEndpointRouting = false
                )
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            services.AddMemoryCache();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }


        /// <summary>
        /// Configure the HTTP request pipeline. 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, ITetriSettings settings, IAppLogic appLogic)
        {
            // register custom error pages
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


            loggerFactory.AddFile(settings.LogPath);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            appLogic.Start();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
