namespace Tetrifact.Core
{
    public interface IPackageDiffService
    {
        /// <summary>
        /// Compares downstream package to an upstream package and finds files that the downstream has that is not no in the upstream. Does not compare the other way.
        /// </summary>
        /// <param name="packageA"></param>
        /// <param name="packageB"></param>
        /// <returns></returns>
        PackageDiff GetDifference(string upstreamPackageId, string downstreamPackageId);
    }
}
