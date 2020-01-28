using System;
using System.Collections.Generic;

namespace Tetrifact.Core
{
    /// <summary>
    /// A unique collection of files which can be retrieved as a single unit.
    /// </summary>
    public class Package
    {
        #region FIELDS

        private string _description = String.Empty;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The unique name of the package, corresponds to folder name in "/packages" folder.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Hash of all file hashes in package. Files must be sorted alphabetically by full path + name, their hash strings concatenated and then the resulting string hashed.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Optional free text field for manifest. Returns an emptry string if empty.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value ?? string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public HashSet<string> Tags { get; set; }

        /// <summary>
        /// True if content of manifest has been diffed against other packages.
        /// </summary>
        public bool IsDiffed { get; set; }

        #endregion

        #region CTORS

        public Package()
        {
            this.Tags = new HashSet<string>();
            this.CreatedUtc = DateTime.UtcNow;
        }

        #endregion
    }
}
