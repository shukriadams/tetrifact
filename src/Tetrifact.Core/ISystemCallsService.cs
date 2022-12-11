namespace Tetrifact.Core
{
    public interface ISystemCallsService
    {
        /// <summary>
        /// Returns files count as long digit in stdout
        /// </summary>
        /// <returns></returns>
        ShellResult GetRepoFilesCount();

        /// <summary>
        /// disk space used - size of /repository dir
        /// du -b --max-depth=0 /path/to/scan 
        /// returns 1 line with value in bytes in stdout, example :
        /// 86123      /path/to/scan
        /// </summary>
        /// <returns></returns>
        ShellResult GetRepoFilesSize();
    }
}
