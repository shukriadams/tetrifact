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
        private readonly IIndexReadService _indexReader;
        private readonly ILogger<IPackageDiffService> _log;

        public PackageDiffService(ISettings settings, IFileSystem filesystem, IIndexReadService indexReader, ILogger<IPackageDiffService> log)
        {
            _settings = settings;
            _fileSystem = filesystem;
            _indexReader = indexReader;
            _log = log;
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
                _log.LogInformation($"Generating diff between \"{upstreamPackageId}\" and \"{downstreamPackageId}\".");

                Manifest downstreamPackage = _indexReader.GetExpectedManifest(downstreamPackageId);
                Manifest upstreamPackage = _indexReader.GetExpectedManifest(upstreamPackageId);

                List<ManifestItem> diffs = new List<ManifestItem>();
                List<ManifestItem> common = new List<ManifestItem>();

                DateTime start = DateTime.UtcNow;

                int nrOfThreads = _settings.WorkerThreadCount;
                int[] threadsCounterArray = new int[] { _settings.WorkerThreadCount };
                for (int i = 0 ; i < threadsCounterArray.Length ; i ++)
                    threadsCounterArray[i] = i;

                int threadCap = nrOfThreads;
                int blockSize = downstreamPackage.Files.Count / nrOfThreads;
                if (downstreamPackage.Files.Count % nrOfThreads != 0)
                    blockSize ++;

                // break downstream manifest's files into blocks, process each block on its own thread, look for matches against upstream manifest
                threadsCounterArray.AsParallel().WithDegreeOfParallelism(_settings.ArchiveCPUThreads).ForAll(async delegate (int thread)
                {
                    int startIndex = thread * blockSize;

                    for (int i = 0; i < blockSize; i++)
                    {
                        if (i + startIndex >= downstreamPackage.Files.Count)
                            break;

                        ManifestItem bItem = downstreamPackage.Files[i + startIndex];

                        if (upstreamPackage.Files.Any(r => r.Hash.Equals(bItem.Hash)))
                        {
                            lock (common)
                                common.Add(bItem);
                        }
                        else
                        {
                            lock (diffs)
                                diffs.Add(bItem);
                        }
                    }
                });

                diff = new PackageDiff
                {
                    GeneratedOnUTC = DateTime.UtcNow,
                    Common = common,
                    UpstreamPackageId = upstreamPackageId,
                    DownstreamPackageId = downstreamPackageId,
                    Difference = diffs.GroupBy(p => p.Path).Select(p => p.First()).ToList() // get distinct by path
                };

                _log.LogInformation($"Generated diff for upstream {upstreamPackageId} and downstream {downstreamPackageId}, tool {(DateTime.UtcNow - start).TotalSeconds} seconds");

                try
                {
                    // todo : lock file system to prevent conurrent writes
                    _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(diffFilePath));
                    _fileSystem.File.WriteAllText(diffFilePath, JsonConvert.SerializeObject(diff));
                }
                catch(Exception ex)
                { 
                    // add context then rethrow
                    throw new Exception($"Unexpected error writing diff between packages {upstreamPackageId} and {downstreamPackageId}", ex);
                }
            }

            return diff;
        }
    }
}
