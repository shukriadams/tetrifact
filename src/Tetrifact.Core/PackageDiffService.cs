using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        public PackageDiffService(ISettingsProvider settingsProvider, IFileSystem filesystem, IIndexReadService indexReader, IMemoryCache cache, ILogger<IPackageDiffService> log)
        {
            _settings = settingsProvider.Get();
            _fileSystem = filesystem;
            _indexReader = indexReader;
            _log = log;
            _cache = cache;
        }

        public PackageDiff GetDifference (string upstreamPackageId, string downstreamPackageId)
        {
            string diffFilePath = Path.Join( _settings.PackageDiffsPath, Obfuscator.Cloak(downstreamPackageId), Obfuscator.Cloak(upstreamPackageId));

            PackageDiff diff = null;

            // check if another process is busy with this diff
            string cacheKey = $"{upstreamPackageId}_diffkey__{downstreamPackageId}";

            DateTime startWait = DateTime.Now;
            int timeoutseconds = 1000 * 60 * 10; // 10 minutes
            bool alertDefer = true;
            while (_cache.Get(cacheKey) != null)
            {
                if (alertDefer)
                {
                    _log.LogInformation($"Diff already in progress for {cacheKey}, waiting until done");
                    alertDefer = false;
                }

                if ((DateTime.Now - startWait).TotalSeconds > timeoutseconds)
                    throw new TimeoutException($"Took more than {timeoutseconds} seconds to wait for diff key {cacheKey}.");

                Thread.Sleep(1000);
            } 

            if (upstreamPackageId == downstreamPackageId)
                throw new InvalidDiffComparison($"Cannot compare package {upstreamPackageId} to itself");

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
                try 
                {
                    _cache.Set(cacheKey, new object());

                    _log.LogInformation($"Generating diff between \"{upstreamPackageId}\" and \"{downstreamPackageId}\".");

                    Manifest downstreamPackage = _indexReader.GetExpectedManifest(downstreamPackageId);
                    Manifest upstreamPackage = _indexReader.GetExpectedManifest(upstreamPackageId);

                    List<ManifestItem> diffs = new List<ManifestItem>();
                    List<ManifestItem> common = new List<ManifestItem>();

                    DateTime start = DateTime.UtcNow;

                    downstreamPackage.Files.AsParallel().WithDegreeOfParallelism(_settings.WorkerThreadCount).ForAll(async delegate (ManifestItem bItem)
                    {
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
                        if (!_fileSystem.File.Exists(diffFilePath))
                            _fileSystem.File.WriteAllText(diffFilePath, JsonConvert.SerializeObject(diff));
                    }
                    catch (Exception ex)
                    {
                        // add context then rethrow
                        throw new Exception($"Unexpected error writing diff between packages {upstreamPackageId} and {downstreamPackageId}", ex);
                    }
                }
                finally
                {
                    _cache.Remove(cacheKey);
                }

            }

            return diff;
        }
    }
}
