using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class TestIndexReader : IIndexReader
    {
        #region FIELDS 

        private Dictionary<string, string> _matchingHashPackage = new Dictionary<string, string>();

        #endregion

        #region PROPERTIES

        public IEnumerable<string> Test_Indexes { get; set; }

        public bool Test_PackageIdExists { get; set; }

        public Manifest Test_Manifest { get; set; }

        public Stream Test_PackageItem { get; set; }

        public Stream Test_PackageArchive { get; set; }

        #endregion

        #region METHODS

        public IEnumerable<string> GetPackageIds()
        {
            return this.Test_Indexes;
        }

        public bool PackageNameInUse(string id)
        {
            return this.Test_PackageIdExists;
        }

        public void AddHash(string path, string hash, string package)
        {
            _matchingHashPackage.Add(string.Format("{0}:{1}", path, hash), package);
        }

        public void Initialize()
        {
            // no need to do anything here
        }

        public Manifest GetManifest(string packageId)
        {
            return this.Test_Manifest;
        }

        public GetFileResponse GetFile(string id)
        {
            return new GetFileResponse(this.Test_PackageItem, id);
        }

        public Stream GetPackageAsArchive(string packageId)
        {
            return this.Test_PackageArchive;
        }

        public void PurgeOldArchives()
        {
            // do nothing
        }

        public int GetPackageArchiveStatus(string packageId)
        {
            return 0;
        }

        public void DeletePackage(string packageId)
        {
            // do nothing
        }

        public void Clean()
        {
            // do nothing
        }

        public void AddTag(string packageId, string tag)
        {
            
        }

        public void RemoveTag(string packageId, string tag)
        {
            
        }

        #endregion
    }
}
