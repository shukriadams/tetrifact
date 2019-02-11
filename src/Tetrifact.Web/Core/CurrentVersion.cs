using System.IO;
using System.Reflection;

namespace Tetrifact.Web
{
    public static class CurrentVersion
    {
        private static string _version;

        static CurrentVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Tetrifact.Web.currentVersion.txt")) 
            using (StreamReader reader = new StreamReader(stream))
            {
                _version = reader.ReadToEnd();
                if (_version.StartsWith("!"))
                    _version = string.Empty;
            }
        }

        public static string Get()
        {
            return _version;
        }
    }
}
