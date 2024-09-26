namespace Tetrifact.Web
{
    /// <summary>
    /// Carries data for /Shared/_Layout.cshtml
    /// </summary>
    public class LayoutViewModel
    {
        public string ThemeClass { get; set; } = string.Empty;

        public string ServerName { get; set; } = string.Empty;

        public string ServerSecondaryName { get; set; } = string.Empty;

        /// <summary>
        /// Optional. A search term that has already been run. Used to show term again.
        /// </summary>
        public string Search { get; set; } = string.Empty;

        public string PageTitle { get; set; } = string.Empty;
    }
}
