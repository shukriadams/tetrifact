﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Tetrifact.Core;
using System.Text;
using Tetrifact.Dev;
using System.Linq;

namespace Tetrifact.DevUtils
{
    public class DiffMethodComparer
    {
        ITetriSettings Settings;
        IPackageCreate PackageCreate;
        IIndexReader IndexReader;

        public DiffMethodComparer(ITetriSettings settings, IIndexReader indexReader, IPackageCreate packageCreate)
        {
            this.Settings = settings;
            this.IndexReader = indexReader;
            this.PackageCreate = packageCreate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="passes"></param>
        /// <param name="initialSize"></param>
        /// <param name="delta"></param>
        public void Compare(int passes = 3, int initialSize = 1024, int delta = 512)
        {
            StringBuilder times = new StringBuilder();
            times.AppendLine($"Passes : {passes}, initialSize {initialSize}, delta {delta} {Environment.NewLine}");

            foreach (DiffMethods diffMethod in Enum.GetValues(typeof(DiffMethods)))
            {
                Settings.DiffMethod = diffMethod;
                IList<DummyFile> inFiles = new List<DummyFile>() {
                    new DummyFile
                    {
                        Data = DataHelper.GetRandomData(initialSize),
                        Path = Guid.NewGuid().ToString()
                    }
                };

                // need project per diffMethod to keep their comparitive diffing separate
                IProjectService projectService = new Core.ProjectService(Settings, new MemoryLogger<IProjectService>());
                string projectName = diffMethod.ToString();
                projectService.Create(projectName);

                for (int i = 0; i < passes; i++)
                {
                    string packageName = $"AddZipLinked{i}{diffMethod}";

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    // create package from files array, zipped up
                    this.PackageCreate.CreateWithValidation(new PackageCreateArguments
                    {
                        Id = packageName,
                        IsArchive = true,
                        Project = projectName,
                        Files = FormFileHelper.FromStream(ArchiveHelper.ZipStreamFromFiles(inFiles), "archive.zip")
                    });

                    sw.Stop();
                    times.AppendLine($"{diffMethod} add {i}, took {sw.Elapsed}");
                    sw.Restart();

                    // retrieve this passes' package and manifest
                    Stream outZip = this.IndexReader.GetPackageAsArchive(projectName, packageName);
                    sw.Stop();
                    times.AppendLine($"{diffMethod} retrieve {i}, took {sw.Elapsed} ms");

                    Manifest manifest = this.IndexReader.GetManifest(projectName, packageName);
                    times.AppendLine($"{diffMethod} pass {i}, {manifest.Compressed}% saved. {Environment.NewLine}");
                    outZip.Dispose();

                    // update files for next pass
                    foreach (DummyFile file in inFiles)
                        file.Data = DataHelper.GetRandomData(delta).Concat(file.Data).ToArray();
                }
            }

            string outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "diff-performance.txt");
            File.WriteAllText(outPath, times.ToString());
            Console.WriteLine($"Diff comparison done, results written to {outPath}.");
        }

    }
}