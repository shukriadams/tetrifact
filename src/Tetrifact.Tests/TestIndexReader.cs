using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class TestIndexReader : IIndexReader
    {
        #region FIELDS 

        private Dictionary<string, string> _matchingHashPackage = new Dictionary<string, string>();

        #endregion

        #region PROPERTIES

        public static IEnumerable<string> Test_Indexes { get; set; }

        public bool Test_PackageIdExists { get; set; }

        public Manifest Test_Manifest { get; set; }

        public Stream Test_PackageItem { get; set; }

        public Stream Test_PackageArchive { get; set; }

        #endregion

        #region METHODS

        public IEnumerable<string> GetPackageIds(int pageIndex, int pageSize)
        {
            return Test_Indexes;
        }

        public bool PackageNameInUse(string id)
        {
            return this.Test_PackageIdExists;
        }

        public void AddHash(string path, string hash, string package)
        {
            _matchingHashPackage.Add($"{path}:{hash}", package);
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

        public string GetPackageArchiveTempPath(string packageId)
        {
            return $"{packageId}.zip.tmp";
        }

        public int GetPackageArchiveStatus(string packageId)
        {
            return 0;
        }

        public void DeletePackage(string packageId)
        {
            // do nothing
        }

        public void CleanRepository()
        {
            // do nothing
        }

        public void AddTag(string packageId, string tag)
        {
            
        }

        public void RemoveTag(string packageId, string tag)
        {
            
        }

        public IEnumerable<string> GetAllPackageIds()
        {
            return Test_Indexes;
        }

        #endregion
    }
}
