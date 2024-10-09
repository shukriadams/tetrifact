namespace Tetrifact.Core
{
    public enum ArchivingModes
    {
        Default,    // internal dotnet zip compressio
        SevenZip    // seven zip. requires external 7zip binary. 7zip support is still experimental.
    }
}