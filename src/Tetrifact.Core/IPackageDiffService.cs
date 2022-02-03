namespace Tetrifact.Core
{
    public interface IPackageDiffService
    {
        /// <summary>
        /// Gets an object representing the file differences between two packages.
        /// </summary>
        /// <param name="packageA"></param>
        /// <param name="packageB"></param>
        /// <returns></returns>
        PackageDiff GetDifference(string packageA, string packageB);
    }
}
