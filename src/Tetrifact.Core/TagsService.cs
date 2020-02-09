namespace Tetrifact.Core
{
    public class TagsService : ITagsService
    {
        #region FIELDS

        private readonly IPackageList _packageList;

        private readonly IIndexReader _indexReader;

        #endregion

        #region CTORS

        public TagsService(IIndexReader indexReader, IPackageList packageList)
        {
            _indexReader = indexReader;
            _packageList = packageList;
        }

        #endregion

        #region METHODS

        public void AddTag(string project, string packageId, string tag)
        {
            try 
            {
                WriteLock.Instance.WaitUntilClear(project);

                Package package = _indexReader.GetPackage(project, packageId);
                if (package == null)
                    throw new PackageNotFoundException(packageId);

                if (package.Tags.Contains(tag))
                    return;

                Transaction transaction = new Transaction(_indexReader, project);

                package.Tags.Add(tag);

                transaction.AddPackage(package);
                transaction.Commit();

                _packageList.Clear(project);

                return;

            } finally {
                WriteLock.Instance.Clear(project);
            }
        }

        public void RemoveTag(string project, string packageId, string tag)
        {
            try
            {
                WriteLock.Instance.WaitUntilClear(project);

                // get current manifest
                Package package = _indexReader.GetPackage(project, packageId);

                if (package == null)
                    throw new PackageNotFoundException(packageId);

                // create new transaction

                if (!package.Tags.Contains(tag))
                    return;

                package.Tags.Remove(tag);


                Transaction transaction = new Transaction(_indexReader, project);
                transaction.AddPackage(package);
                transaction.Commit();

                // flush in-memory tags
                _packageList.Clear(project);

                return;
            }
            finally 
            {
                WriteLock.Instance.Clear(project);
            }
        }

        #endregion
    }
}
