using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Tetrifact.DevUtils
{
    class PackageGenerator
    {
        /// <summary>
        /// Generates given number of packages in the path. Wipes the target path
        /// </summary>
        /// <param name="count"></param>
        public static void CreatePackages(int count, string path)
        {

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);
            List<string> packages = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string packageName = Guid.NewGuid().ToString();
                string packageFolder = Path.Combine(path, packageName);
                Directory.CreateDirectory(packageFolder);
                packages.Add(packageName);
            }

            string indexData = JsonConvert.SerializeObject(packages);
            File.WriteAllText(Path.Join(path, "index.json"), indexData);
        }

        public static void GetIndexes(string path)
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
