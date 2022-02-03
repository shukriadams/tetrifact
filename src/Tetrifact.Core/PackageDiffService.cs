using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Tetrifact.Core
{
    public class PackageDiffService : IPackageDiffService
    {
        private readonly ITetriSettings _settings;
        private readonly IFileSystem _fileSystem;
        private readonly IIndexReader _indexReader;
        private readonly ILogger<IPackageDiffService> _logger;

        public PackageDiffService(ITetriSettings settings, IFileSystem fileSystem, IIndexReader indexReader, ILogger<IPackageDiffService> logger)
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

        public PackageDiff GetDifference (string packageA, string packageB)
        {
            Tuple<string, string> sorted = this.Sort(packageA, packageB);
            packageA = sorted.Item1;
            packageB = sorted.Item2;
           
            string diffFilePath = Path.Join( _settings.PackageDiffsPath, Obfuscator.Cloak(packageA), Obfuscator.Cloak(packageB));

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
                Manifest manifestA = _indexReader.GetExpectedManifest(packageA);
                Manifest manifestB = _indexReader.GetExpectedManifest(packageB);

                diff = new PackageDiff
                { 
                    PackageA = packageA,
                    PackageB = packageB,
                    Files = manifestA.Files.Where(p => manifestB.Files.All(p2 => p2.Hash != p.Hash)).ToList()
                };

                try
                {
                    // todo : lock file system to prevent conurrent writes
                    _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(diffFilePath));
                    _fileSystem.File.WriteAllText(diffFilePath, JsonConvert.SerializeObject(diff));
                }
                catch(Exception ex)
                { 
                    _logger.LogError($"Unexpected error writing diff between packages {packageA} and {packageB}", ex);
                }

            }

            return diff;
        }
    }
}
