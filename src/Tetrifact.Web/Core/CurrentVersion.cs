using System.IO;
using System.Reflection;

namespace Tetrifact.Web
{
    /// <summary>
    /// Reads current version from text file. This is used to display current version in footer on UI. Version is read from text file that
    /// is written during build process, and contains the git tag the build is triggered from.
    /// </summary>
    public static class CurrentVersion
    {
        #region FIELDS

        private readonly static string _version;

        #endregion

        #region CTORS

        /// <summary>
        /// Loads version once on instantiate.
        /// </summary>
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

        #endregion

        #region METHODS

        /// <summary>
        /// Gets version.
        /// </summary>
        /// <returns></returns>
        public static string Get()
        {
            return _version;
        }

        #endregion
    }
}
