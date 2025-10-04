using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Tests
{
    /// <summary>
    /// Base class for any type which requires concrete file system structures in place
    /// </summary>
    public abstract class FileSystemBase : TestBase
    {
        #region FIELDS
        
        protected ThreadDefault ThreadDefault;
        
        #endregion

        #region CTORS

        public FileSystemBase()
        {
            ThreadDefault = new ThreadDefault();
        }

        #endregion
    }
}
