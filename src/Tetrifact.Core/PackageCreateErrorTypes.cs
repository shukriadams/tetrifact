namespace Tetrifact.Core
{
    public enum PackageCreateErrorTypes
    {
        MissingValue,
        PackageExists,
        InvalidFileCount,
        InvalidArchiveFormat,
        InvalidDiffAgainstPackage,
        UnexpectedError
    }
}
