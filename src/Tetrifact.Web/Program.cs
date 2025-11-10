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
            // start logging as soon as app starts, we want to use log times to catch bottlenecks.  Loading continues in Startup.cs.

            Console.WriteLine("*********************************************************************");
            Console.WriteLine("TETRIFACT server starting");
            Console.WriteLine("");

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>{ 
                    // add explicit console.writeline output to all log writes
                    logging.AddConsole(console =>
                    {
                        // add timestamp to logout
                        console.TimestampFormat = "[HH:mm:ss] ";
                    });

                    logging.AddCustomFormatter();
                })
                .UseStartup<Startup>();

            bool use_IIS = Environment.GetEnvironmentVariable("TETRIFACT_USE_IIS") == "true";
            bool useHTTPS = EnvironmentArgsHelper.GetAsBool("TETRIFACT_USE_HTTPS");
            string httpsCertPath = Environment.GetEnvironmentVariable("TETRIFACT_HTTPS_CERT_PATH");
            int port = EnvironmentArgsHelper.GetAsInt("TETRIFACT_PORT", 5000);

            if (useHTTPS && string.IsNullOrEmpty(httpsCertPath)){
                Console.WriteLine("TETRIFACT_HTTPS_CERT_PATH env var not set, cannot use HTTPS");
                useHTTPS = false;
            }

            if (!use_IIS)
            {
                builder.UseKestrel(options =>
                {

                    options.Listen(IPAddress.Any, port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        if (useHTTPS)
                            listenOptions.UseHttps(httpsCertPath);
                    });

                    Console.WriteLine($"Port bound ({Global.StartTimeUtc.Ago(true)})");

                    // SECURITY WARNING : the limit on attachment part size is removed to support large builds. 
                    options.Limits.MaxRequestBodySize = long.MaxValue;
                    options.Limits.MaxRequestBufferSize = long.MaxValue;
                    options.Limits.MaxRequestLineSize = int.MaxValue;
                });

                Console.WriteLine($"Kestrel loaded ({Global.StartTimeUtc.Ago(true)})");
            }

            return builder;
        }
    }
}
