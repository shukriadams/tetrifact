namespace Tetrifact.Core
{
    public class TagsService : ITagsService
    {
        #region FIELDS

        private readonly ISettings _settings;

        private readonly IPackageList _packageList;

        private readonly IIndexReader _indexReader;

        #endregion

        #region CTORS

        public TagsService(ISettings settings, IIndexReader indexReader, IPackageList packageList)
        {
            _indexReader = indexReader;
            _settings = settings;
            _packageList = packageList;
        }

        #endregion

        #region METHODS

        public void AddTag(string project, string packageId, string tag)
        {
            try 
            {
                WriteLock.Instance.WaitUntilClear(project);

                Manifest manifest = _indexReader.GetManifest(project, packageId);
                if (manifest == null)
                    throw new PackageNotFoundException(packageId);

                if (manifest.Tags.Contains(tag))
                    return;

                Transaction transaction = new Transaction(_settings, _indexReader, project);

                manifest.Tags.Add(tag);

                transaction.AddManifest(manifest);
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
                Manifest manifest = _indexReader.GetManifest(project, packageId);

                if (manifest == null)
                    throw new PackageNotFoundException(packageId);

                // create new transaction

                if (!manifest.Tags.Contains(tag))
                    return;

                manifest.Tags.Remove(tag);


                Transaction transaction = new Transaction(_settings, _indexReader, project);
                transaction.AddManifest(manifest);
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
