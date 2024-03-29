﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using Tetrifact.Core;
using Troschuetz.Random;

namespace Tetrifact.DevUtils
{
    public class PackageGenerator
    {
        IPackageCreateService _packageServices;

        public PackageGenerator(IPackageCreateService packageServices)
        {
            _packageServices = packageServices;
        }

        /// <summary>
        /// Generates given number of packages in the path. Wipes the target path
        /// </summary>
        /// <param name="count"></param>
        public void CreatePackages(int count, int maxFiles, int maxFileSize, string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);
            List<string> packages = new List<string>();

            TRandom random = new TRandom();
            for (int i = 0; i < count; i++)
            {
                List<PackageCreateItem> files = new List<PackageCreateItem>();
                int filesToAdd = random.Next(0, maxFiles);
                for (int j = 0; j < filesToAdd; j++)
                {
                    byte[] buffer = new byte[maxFileSize];
                    random.NextBytes(buffer);
                    Stream file = new MemoryStream(buffer);
                    files.Add(new PackageCreateItem(file, $"folder{Guid.NewGuid()}/{Guid.NewGuid()}"));
                }
                PackageCreateArguments package = new PackageCreateArguments {
                    Id = Guid.NewGuid().ToString(),
                    Files = files
                };
                _packageServices.Create(package);
                Console.WriteLine($"Generated package {package.Id}");
            }

        }

        public void GetIndexes(string path)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string[] packages = Directory.GetFiles(path);

            sw.Stop();
            Console.WriteLine("FS read Elapsed={0}", sw.Elapsed);

            sw.Reset();
            sw.Start();

            List<string> otherPackages = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Path.Join(path, "index.json")));

            sw.Stop();
            Console.WriteLine("JSON load Elapsed={0}", sw.Elapsed);
        }
    }
}
