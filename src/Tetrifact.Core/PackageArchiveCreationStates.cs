namespace Tetrifact.Core
{
    public enum PackageArchiveCreationStates 
    {
        Queued,
        ArchiveGenerating,
        Processed_PackageNotFound,
        Processed_ArchiveAvailable,
        Processed_ArchiveNotGenerated
    }
}
