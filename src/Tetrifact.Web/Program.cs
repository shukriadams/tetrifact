using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Tetrifact.Core;

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
                    logging.AddCustomFormatter();
                })
                .UseStartup<Startup>();
                

            bool isIIS = Environment.GetEnvironmentVariable("IS_IIS") == "true";
            bool useHTTPS = EnvironmentArgsHelper.GetAsBool("TETRIFACT_USE_HTTPS");
            string httpsCertPath = Environment.GetEnvironmentVariable("TETRIFACT_HTTPS_CERT_PATH");
            int port = EnvironmentArgsHelper.GetAsInt("TETRIFACT_PORT", 5000);

            if (useHTTPS && string.IsNullOrEmpty(httpsCertPath)){
                Console.WriteLine("TETRIFACT_HTTPS_CERT_PATH env var not set, cannot use HTTPS");
                useHTTPS = false;
            }

            if (!isIIS)
            {
                builder.UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        if (useHTTPS)
                            listenOptions.UseHttps(httpsCertPath);
                    });

                    // SECURITY WARNING : the limit on attachment part size is removed to support large builds. 
                    options.Limits.MaxRequestBodySize = long.MaxValue;
                    options.Limits.MaxRequestBufferSize = long.MaxValue;
                    options.Limits.MaxRequestLineSize = int.MaxValue;
                });
            }

            return builder;
        }
    }
}
