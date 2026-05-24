namespace Tetrifact.Core
{
    public class RepositoryAddResponse
    {
        /// <summary>
        /// If true, file is unique in repository, ie, its hash did not already exist. If false, file is a dupe
        /// </summary>
        public bool IsUnique {get ;set;}
        
        /// <summary>
        /// If not a dupe, the size the file took on disk.
        /// </summary>
        public long SizeOnDisk {get;set;}
    }    
}

