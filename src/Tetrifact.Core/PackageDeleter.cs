using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tetrifact.Core
{
    public class PackageDeleter : IPackageDeleter
    {
        private readonly IIndexReader _indexReader;

        private ITypeProvider _typeProvider;

        public PackageDeleter(IIndexReader indexReader, ITypeProvider typeProvider) 
        {
            _typeProvider = typeProvider;
            _indexReader = indexReader;
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

                Transaction transaction = new Transaction(_indexReader, project);
                

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
                        string dependantPackage = Obfuscator.Decloak((Path.GetFileName(dependant).Replace($"dep_{packageObfuscated}_", string.Empty)).Replace("_", string.Empty));

                        IPackageCreate packageCreate = _typeProvider.GetInstance<IPackageCreate>();

                        // decouple dependend from package-to-delete, link it to package-to-delete's parent instead
                        packageCreate.CreateFromExisting(project, dependantPackage, packageToDeleteManifest.DependsOn, transaction);
                    }
                }

                // after we've recreated dependents from the content to be deleted, delete the content
                transaction.Remove(package);

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
