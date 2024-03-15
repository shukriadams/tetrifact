namespace Tetrifact
{
    public interface IPackageFile
    {
        string Path { get; set; }

        string Hash { get; set; }
    }
}
