using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public class PackageDeleter : IPackageDeleter
    {
        private readonly IIndexReader _indexReader;

        private readonly ITetriSettings _settings;

        private readonly ILogger<IPackageDeleter> _logger;

        private readonly ILogger<Workspace> _workspaceLogger;

        public PackageDeleter(IIndexReader indexReader, ITetriSettings settings, ILogger<IPackageDeleter> logger, ILogger<Workspace> workspaceLogger) 
        {
            _workspaceLogger = workspaceLogger;
            _indexReader = indexReader;
            _settings = settings;
            _logger = logger;
        }

        public async Task Delete(string project, string packageId)
        {
            LockRequest lockRequest = new LockRequest();
            
            try
            {
                await lockRequest.Get();

                Manifest packageToDeleteManifest = _indexReader.GetManifest(project, packageId);
                if (packageToDeleteManifest == null)
                    throw new PackageNotFoundException(packageId);

                Transaction transaction = new Transaction(_settings, _indexReader, project);
                transaction.RemoveManifestPointer(packageId);
                transaction.RemoveShardPointer(packageId);

                // find dependencies, there should be only one
                DirectoryInfo currentTransaction = _indexReader.GetActiveTransactionInfo(project);
                IEnumerable<string> depedencies = currentTransaction.GetFiles().Where(r => r.Name.StartsWith($"dep_{packageId}_")).Select(r => r.FullName);

                // reconstitute 
                foreach (string dependencyFile in depedencies)
                {
                    string dependencyPackage = Path.GetFileName(dependencyFile.Replace($"dep_{packageId}_", string.Empty));
                    IWorkspace workspace = new Workspace(_indexReader, _settings, _workspaceLogger);
                    workspace.Initialize(project);

                    // rehydrate entire package to temp location
                    Manifest manifest = _indexReader.GetManifest(project, dependencyPackage);
                    foreach (ManifestItem file in manifest.Files)
                    {
                        using (Stream sourceFile = _indexReader.GetFile(project, file.Id).Content)
                        {
                            workspace.AddIncomingFile(sourceFile, file.Path);
                        }
                    }
                    workspace.StageAllFiles(dependencyPackage, packageToDeleteManifest.DependsOn);
                    workspace.Commit(project, dependencyPackage, packageToDeleteManifest.DependsOn, transaction);
                }

                // merge patches into next package
                transaction.Commit();
            }
            finally
            {
                LinkLock.Instance.Release();
            }
        }
    }
}
