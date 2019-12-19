using System.Threading.Tasks;

namespace Tetrifact.Core
{
    public interface IPackageDeleter
    {
        /// <summary>
        /// Marking a package for delete is the first stage of deleting a package. What this does it :
        /// 1 - Finds the next mainline package that is dependent on this one. In that, create a mergeset which is basically applying that package's patches to the one being deleted.
        /// 2 - Mark this package's manifest to delete.
        /// 3 - Find all dead-end packages linked to this one, and mark their manifests for deleting as well.
        /// </summary>
        /// <param name="package"></param>
        Task Delete(string project, string package);
    }
}
