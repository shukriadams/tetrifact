using Microsoft.Extensions.Logging;
using System;
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

        public PackageDeleter(IIndexReader indexReader, ITetriSettings settings, ILogger<IPackageDeleter> logger, ILogger<IPackageCreate> workspaceLogger) 
        {
            _packageCreateLogger = workspaceLogger;
            _indexReader = indexReader;
            _settings = settings;
            _logger = logger;
        }

        public void Delete(string project, string package)
        {
            try
            {
                LinkLock.Instance.WaitUntilClear();

                Manifest packageToDeleteManifest = _indexReader.GetManifest(project, package);
                if (packageToDeleteManifest == null)
                    throw new PackageNotFoundException(package);

                Transaction transaction = new Transaction(_settings, _indexReader, project);
                transaction.RemoveManifestPointer(package);
                transaction.RemoveShardPointer(package);

                // find dependencies, there should be only one
                DirectoryInfo currentTransaction = _indexReader.GetActiveTransactionInfo(project);
                IEnumerable<string> depedencies = currentTransaction.GetFiles().Where(r => r.Name.StartsWith($"dep_{package}_")).Select(r => r.FullName);

                // reconstitute 
                foreach (string dependencyFile in depedencies)
                {
                    string dependencyPackage = Path.GetFileName(dependencyFile.Replace($"dep_{package}_", string.Empty));
                    IPackageCreate packageCreate = new PackageCreate(_indexReader, _packageCreateLogger, _settings);
                    packageCreate.CreateFromExisting(project, dependencyPackage, packageToDeleteManifest.DependsOn, transaction);
                }

                // merge patches into next package
                transaction.Commit();
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Unexpected error deleting package {package} {ex}");
            }
            finally
            {
                LinkLock.Instance.Clear();
            }
        }
    }
}
