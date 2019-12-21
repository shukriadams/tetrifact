using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Tetrifact.Core
{
    public class TagsService : ITagsService
    {
        #region FIELDS

        private readonly ITetriSettings _settings;

        private readonly ILogger<ITagsService> _logger;

        private readonly IPackageList _packageList;

        private readonly IIndexReader _indexReader;

        #endregion

        #region CTORS

        public TagsService(ITetriSettings settings, ILogger<ITagsService> logger, IIndexReader indexReader, IPackageList packageList)
        {
            _indexReader = indexReader;
            _settings = settings;
            _logger = logger;
            _packageList = packageList;
        }

        #endregion

        #region METHODS

        public void AddTag(string project, string packageId, string tag)
        {
            try 
            {
                LinkLock.Instance.WaitUntilClear();

                // get current manifest
                Manifest manifest = _indexReader.GetManifest(project, packageId);

                if (manifest == null)
                    throw new PackageNotFoundException(packageId);

                // create new transaction

                if (manifest.Tags.Contains(tag))
                    return;

                Transaction transaction = new Transaction(_settings, _indexReader, project);

                manifest.Tags.Add(tag);
                string fileName = $"{Guid.NewGuid()}_{packageId}";
                File.WriteAllText(Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment, fileName), JsonConvert.SerializeObject(manifest));
                transaction.AddManifestPointer(packageId, fileName);


                // flush in-memory tags
                transaction.Commit();
                _packageList.Clear(project);

                return;

            } finally {
                LinkLock.Instance.Clear();
            }

        }

        public void RemoveTag(string project, string packageId, string tag)
        {
            try
            {
                LinkLock.Instance.WaitUntilClear();

                // get current manifest
                Manifest manifest = _indexReader.GetManifest(project, packageId);

                if (manifest == null)
                    throw new PackageNotFoundException(packageId);

                // create new transaction

                if (!manifest.Tags.Contains(tag))
                    return;

                Transaction transaction = new Transaction(_settings, _indexReader, project);

                manifest.Tags.Remove(tag);
                string fileName = $"{Guid.NewGuid()}_{packageId}";
                File.WriteAllText(Path.Combine(_settings.ProjectsPath, Obfuscator.Cloak(project), Constants.ManifestsFragment, fileName), JsonConvert.SerializeObject(manifest));
                transaction.AddManifestPointer(packageId, fileName);


                // flush in-memory tags
                transaction.Commit();
                _packageList.Clear(project);

                return;

            }
            finally 
            {
                LinkLock.Instance.Clear();
            }

        }


        #endregion
    }
}
