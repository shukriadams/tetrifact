namespace Tetrifact.Core
{
    public interface ITetriSettings
    {
        /// <summary>
        /// Path where packages are stored. Each package lives in its own folder.
        /// </summary>
        string PackagePath { get; set; }

        /// <summary>
        /// Folder to store incoming files, temp archives, etc. Wiped on app start.
        /// </summary>
        string TempPath { get; set; }
 
        /// <summary>
        /// Folder to store complete archives. Each archive is named for the package it contains.
        /// </summary>
        string ArchivePath { get; set; }

        /// <summary>
        /// Global path to hash index - this cross-package index is used to determine which package contains a specific hash.
        /// </summary>
        string RepositoryPath { get; set; }

        /// <summary>
        /// Folder tags are written to.
        /// </summary>
        string TagsPath { get; set; }

        /// <summary>
        /// Milliseconds.
        /// </summary>
        int ArchiveAvailablePollInterval { get; set; }
    }
}
