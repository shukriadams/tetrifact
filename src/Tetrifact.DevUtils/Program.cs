using System;
using System.IO;

namespace Tetrifact.DevUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineArgumentParser commandArgs = new CommandLineArgumentParser(args, "--", true);

            if (!commandArgs.Contains("run"))
            {
                Console.WriteLine("Missing --run argument");
                Environment.Exit(1);
            }

            switch (commandArgs.Get("run"))
            {
                case "generatePackages" :
                    if (!commandArgs.Contains("size"))
                    {
                        Console.WriteLine("Missing --size argument");
                        Environment.Exit(1);
                    }

                    PackageGenerator.CreatePackages(
                        Int32.Parse(commandArgs.Get("size")),
                        Path.Join(AppDomain.CurrentDomain.BaseDirectory, "packages"));

                    return;
                case "indexPerf":
                    PackageGenerator.GetIndexes(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "packages"));
                    return;

                default:
                {
                    Console.WriteLine("Unsupported run case");
                    return;
                }
            }

        }
    }
}
