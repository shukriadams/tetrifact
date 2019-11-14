using System.IO;

namespace Tetrifact.Core
{
    public class AppLogic : IAppLogic
    {
        private readonly ITetriSettings _settings;

        public AppLogic(ITetriSettings settings) 
        {
            _settings = settings;
        }

        public void Start() 
        {
            if (!Directory.Exists(_settings.ArchivePath))
                Directory.CreateDirectory(_settings.ArchivePath);

            // force wipe the temp folder when app starts
            try {
                if (Directory.Exists(_settings.TempPath))
                    Directory.Delete(_settings.TempPath, true);
                Directory.CreateDirectory(_settings.TempPath);
            }
            catch (IOException) 
            {
                // if temp is folder is locked, ignore deleting it
            }

            if (!Directory.Exists(_settings.TempPath))
                Directory.CreateDirectory(_settings.TempPath);

            if (!Directory.Exists(_settings.TempBinaries))
                Directory.CreateDirectory(_settings.TempBinaries);

        }
    }
}
