namespace Tetrifact.Core
{
    /// <summary>
    /// List of required authorization levels for API methods.
    /// </summary>
    public enum AuthorizationLevel : int
    {
        None,
        Read,
        Write
    }
}
