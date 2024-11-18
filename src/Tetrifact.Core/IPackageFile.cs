namespace Tetrifact.Core
{
    public interface IPackageFile
    {
        string Path { get; set; }

        string Hash { get; set; }
    }
}
