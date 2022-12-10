namespace Tetrifact.Core
{
    public interface IPackageDiffService
    {
        /// <summary>
        /// Compares downstream package to an upstream package and finds files that the downstream has that is not in the upstream. Does not compare the other way.
        /// Used for partial downloads, where a client will have an upstream package stored locally and wishes to obtain a downstream package by fetching only
        /// diffs, and reconstituting the remainder from local upstream package files.
        /// </summary>
        /// <param name="packageA"></param>
        /// <param name="packageB"></param>
        /// <returns></returns>
        PackageDiff GetDifference(string upstreamPackageId, string downstreamPackageId);
    }
}
