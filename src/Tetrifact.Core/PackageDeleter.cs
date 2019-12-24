using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class PackageDeleter : IPackageDeleter
    {
        private readonly IIndexReader _indexReader;

        private readonly ITetriSettings _settings;

        private readonly ILogger<IPackageDeleter> _logger;

        private readonly ILogger<IPackageCreate> _packageCreateLogger;

        public PackageDeleter(IIndexReader indexReader, ITetriSettings settings, ILogger<IPackageDeleter> logger, ILogger<IPackageCreate> packageCreateLogger) 
        {
            _packageCreateLogger = packageCreateLogger;
            _indexReader = indexReader;
            _settings = settings;
            _logger = logger;
        }

        public void Delete(string project, string package)
        {
            try
            {
                WriteLock.Instance.WaitUntilClear(project);
                
                if (!_indexReader.ProjectExists(project))
                    throw new ProjectNotFoundException(project);

                Manifest packageToDeleteManifest = _indexReader.GetManifest(project, package);
                if (packageToDeleteManifest == null)
                    throw new PackageNotFoundException(package);

                Transaction transaction = new Transaction(_settings, _indexReader, project);
                transaction.Remove(package);

                string packageObfuscated = Obfuscator.Cloak(package);

                // find all dependants, there should be only one
                DirectoryInfo currentTransaction = _indexReader.GetActiveTransactionInfo(project);
                IEnumerable<string> dependants = currentTransaction.GetFiles().Where(r => r.Name.StartsWith($"dep_{packageObfuscated}_")).Select(r => r.FullName);

                if (dependants.Count() == 0)
                {
                    transaction.SetHead(packageToDeleteManifest.DependsOn);
                }
                else 
                {
                    foreach (string dependant in dependants)
                    {
                        string dependantPackage = Obfuscator.Decloak(Path.GetFileName(dependant).Replace($"dep_{packageObfuscated}_", string.Empty));
                        IPackageCreate packageCreate = new PackageCreate(_indexReader, _packageCreateLogger, _settings);
                        packageCreate.CreateFromExisting(project, dependantPackage, packageToDeleteManifest.DependsOn, transaction);
                    }
                }

                // merge patches into next package
                transaction.Commit();
            }

            finally
            {
                WriteLock.Instance.Clear(project);
            }
        }
    }
}
