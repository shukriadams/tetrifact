using System.Collections.Generic;
using System.IO;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    public class TestIndexReader : IIndexReader
    {
        #region FIELDS 

        private readonly Dictionary<string, string> _matchingHashPackage = new Dictionary<string, string>();

        #endregion

        #region PROPERTIES

        public static IEnumerable<string> Test_Indexes { get; set; }

        public bool Test_PackageIdExists { get; set; }

        public Manifest Test_Manifest { get; set; }

        public Stream Test_PackageItem { get; set; }

        public Stream Test_PackageArchive { get; set; }

        public string Test_Head { get; set; }

        public IEnumerable<string> Test_Projects { get; set; }

        #endregion

        #region METHODS

        public IEnumerable<string> GetPackageIds(string project, int pageIndex, int pageSize)
        {
            return Test_Indexes;
        }

        public bool PackageNameInUse(string project, string id)
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

        public Manifest GetManifest(string project, string packageId)
        {
            return this.Test_Manifest;
        }

        public GetFileResponse GetFile(string project, string id)
        {
            return new GetFileResponse(this.Test_PackageItem, id);
        }

        public Stream GetPackageAsArchive(string project, string packageId)
        {
            return this.Test_PackageArchive;
        }

        public void PurgeOldArchives()
        {
            // do nothing
        }

        public string GetPackageArchiveTempPath(string project, string packageId)
        {
            return $"{packageId}.zip.tmp";
        }

        public string GetPackageArchivePath(string project, string packageId)
        {
            return $"{packageId}.zip";
        }

        public int GetPackageArchiveStatus(string project, string packageId)
        {
            return 0;
        }

        public void MarkPackageForDelete(string project, string packageId)
        {
            // do nothing
        }

        public void CleanRepository()
        {
            // do nothing
        }

        public string GetHead(string project) 
        {
            return this.Test_Head;
        }

        public void AddTag(string packageId, string tag)
        {
            
        }

        public void RemoveTag(string packageId, string tag)
        {
            
        }

        public IEnumerable<string> GetAllPackageIds(string project)
        {
            return Test_Indexes;
        }

        public string RehydrateOrResolve(string project, string package, string filePath)
        {
            return null;
        }

        public IEnumerable<string> GetProjects()
        {
            return this.Test_Projects;
        }

        #endregion
    }
}
