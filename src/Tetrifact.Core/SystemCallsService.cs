using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tetrifact.Core
{
    public class SystemCallsService : ISystemCallsService
    {
        #region FIELDS

        private ISettings _settings;

        #endregion

        #region CTORS

        public SystemCallsService(ISettings settings) 
        {
            _settings = settings;
        }

        #endregion

        #region METHODS

        public ShellResult GetRepoFilesCount()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Shell.Run($"find {_settings.RepositoryPath} -type f | wc -l", false, 3600000); // 1 hour timeout, we need to move this to settings

            // return 0 on non-linux systems
            return new ShellResult(0, new List<string> { "0" }, new List<string>() );
        }

        public ShellResult GetRepoFilesSize()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Shell.Run($"du -b --max-depth=0 {_settings.RepositoryPath}", false, 3600000); // 1 hour timeout, we need to move this to settings

            // return 0 on non-linux systems
            return new ShellResult(0, new List<string> { $"0      {_settings.RepositoryPath}" }, new List<string>());
        }

        #endregion
    }
}
