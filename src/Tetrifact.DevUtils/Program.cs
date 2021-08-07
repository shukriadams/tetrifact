using Ninject;
using System;
using System.IO;
using System.Reflection;

namespace Tetrifact.DevUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            StandardKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            CommandLineSwitches commandArgs = new CommandLineSwitches(args, "--", true);

            if (!commandArgs.Contains("run"))
            {
                Console.WriteLine("Missing --run argument, check source code for options");
                Environment.Exit(1);
            }

            switch (commandArgs.Get("run"))
            {
                case "generatePackages" :
                {
                    if (!commandArgs.Contains("size"))
                    {
                        Console.WriteLine("Missing --size argument");
                        Environment.Exit(1);
                    }

                    PackageGenerator packageGenerater = kernel.Get<PackageGenerator>();
                    packageGenerater.CreatePackages(
                        Int32.Parse(commandArgs.Get("size")),
                        10, 
                        10,
                        Path.Join(AppDomain.CurrentDomain.BaseDirectory, "packages"));

                    return;
                }
                case "indexPerf":
                {
                    PackageGenerator packageGenerater = kernel.Get<PackageGenerator>();
                    packageGenerater.GetIndexes(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "packages"));
                    return;
                }

                default:
                {
                    Console.WriteLine("Unsupported run case");
                    return;
                }
            }

        }
    }
}
