using Microsoft.AspNetCore.Mvc;

namespace Tetrifact.Core
{
    public class ProjectCreateArguments
    {
        #region FIELDS

        /// <summary>
        /// Name of project to add package to. The package will be automatically created it it doesn't yet exist.
        /// </summary>
        [FromRoute] public string Project { get; set; }

        /// <summary>
        /// Optional description for package
        /// </summary>
        [FromForm] public string Description { get; set; }

        #endregion
    }
}
