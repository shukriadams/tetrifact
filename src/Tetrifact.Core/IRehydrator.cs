namespace Tetrifact.Core
{
    public interface IRehydrator
    {
        string RehydrateOrResolveFile(string project, string packageId, string filePath);
    }
}
