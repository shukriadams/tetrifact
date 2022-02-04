using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;

namespace Tetrifact.Core
{
    public class PackageDiffService : IPackageDiffService
    {
        private readonly ISettings _settings;
        private readonly IFileSystem _fileSystem;
        private readonly IIndexReader _indexReader;
        private readonly ILogger<IPackageDiffService> _logger;

        public PackageDiffService(ISettings settings, IFileSystem fileSystem, IIndexReader indexReader, ILogger<IPackageDiffService> logger)
        {
            _settings = settings;
            _fileSystem = fileSystem;
            _indexReader = indexReader;
            _logger = logger;
        }

        private Tuple<string, string> Sort(string packageA, string packageB)
        {
            List<string> test = new string[] { packageA, packageB }.ToList();
            test.Sort();
            return new Tuple<string, string>(test[0], test[1]);
        }

        public PackageDiff GetDifference (string upstreamPackageId, string downstreamPackageId)
        {
            string diffFilePath = Path.Join( _settings.PackageDiffsPath, Obfuscator.Cloak(downstreamPackageId), Obfuscator.Cloak(upstreamPackageId));

            PackageDiff diff = null;

            if (_fileSystem.File.Exists(diffFilePath))
            {
                string json = null;
                try 
                {
                    json = _fileSystem.File.ReadAllText(diffFilePath);
                }
                catch(Exception ex)
                { 
                    throw new Exception($"Unexpected error reading diff @ {diffFilePath}", ex);
                }

                diff = JsonConvert.DeserializeObject<PackageDiff>(json);
            }
            else
            { 
                Manifest downstreamPackage = _indexReader.GetExpectedManifest(downstreamPackageId);
                Manifest upstreamPackage = _indexReader.GetExpectedManifest(upstreamPackageId);

                List<ManifestItem> diffs = new List<ManifestItem>();

                DateTime start = DateTime.UtcNow;

                int nrOfThreads = _settings.WorkerThreadCount;
                int threadCap = nrOfThreads;
                int blockSize = downstreamPackage.Files.Count / nrOfThreads;

                ManualResetEvent resetEvent = new ManualResetEvent(false);

                for (int thread = 0; thread < nrOfThreads; thread++)
                {
                    new Thread(delegate ()
                    {
                        try
                        {
                            int startIndex = thread * blockSize;
                            int limit = blockSize;
                            if (thread == nrOfThreads)
                                limit = downstreamPackage.Files.Count % nrOfThreads;

                            for (int i = 0; i < limit; i++)
                            {
                                ManifestItem bItem = downstreamPackage.Files[i + startIndex];
                                if (!upstreamPackage.Files.Any(r => r.Hash.Equals(bItem.Hash)))
                                {
                                    lock (diffs)
                                        diffs.Add(bItem);
                                }
                            }
                        }
                        finally
                        {
                            if (Interlocked.Decrement(ref threadCap) == 0)
                                resetEvent.Set();
                        }
                    }).Start();
                }

                // Wait for threads to finish
                resetEvent.WaitOne();

                diff = new PackageDiff
                {
                    GeneratedOnUTC = DateTime.UtcNow,
                    Taken = (DateTime.UtcNow - start).TotalSeconds,
                    UpstreamPackageId = upstreamPackageId,
                    DownstreamPackageId = downstreamPackageId,
                    Files = diffs.GroupBy(p => p.Path).Select(p => p.First()).ToList() // get distinct by path
                };

                try
                {
                    // todo : lock file system to prevent conurrent writes
                    _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(diffFilePath));
                    _fileSystem.File.WriteAllText(diffFilePath, JsonConvert.SerializeObject(diff));
                }
                catch(Exception ex)
                { 
                    _logger.LogError($"Unexpected error writing diff between packages {upstreamPackageId} and {downstreamPackageId}", ex);
                }

            }

            return diff;
        }
    }
}
