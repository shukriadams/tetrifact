using Ninject;
using System;
using System.Reflection;

namespace Tetrifact.DevUtils
{
    public class Program
    {
        private static StandardKernel _kernel;

        private static void Main(string[] args)
        {
            _kernel = new StandardKernel();
            _kernel.Load(Assembly.GetExecutingAssembly());

            CommandLineSwitches commandArgs = new CommandLineSwitches(args, "--", true);
            if (!commandArgs.Contains("run"))
            {
                Console.WriteLine("Missing --run argument");
                Environment.Exit(1);
            }

            switch (commandArgs.Get("run"))
            {
                case "generatePackages" :
                {
                    GeneratePackages(commandArgs);
                    return;
                }

                default:
                {
                    Console.WriteLine("Unsupported run case");
                    return;
                }
            }
        }

        private static void GeneratePackages(CommandLineSwitches commandArgs) 
        {
            if (!commandArgs.Contains("size"))
            {
                Console.WriteLine("Missing --size argument");
                Environment.Exit(1);
            }

            DiffMethodComparer comparer = _kernel.Get<DiffMethodComparer>();
            comparer.Compare();
        }
    }
}
