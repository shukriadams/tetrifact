﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.Logging;

namespace Tetrifact.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>{ 
                    // add explicit console.writeline output to all log writes
                    logging.AddConsole();
                })
                .UseStartup<Startup>();
                

            bool isIIS = Environment.GetEnvironmentVariable("IS_IIS") == "true";

            if (!isIIS)
            {
                builder.UseKestrel(options =>
                {
                    // SECURITY WARNING : the limit on attachment part size is removed to support large builds. 
                    options.Limits.MaxRequestBodySize = null;
                    options.Limits.MaxRequestBufferSize = null;
                    options.Limits.MaxRequestLineSize = int.MaxValue;

                });
            }

            return builder;
        }
    }
}
