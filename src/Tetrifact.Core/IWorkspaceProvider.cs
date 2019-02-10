namespace Tetrifact.Core
{
    public interface IWorkspaceProvider
    {
        /// <summary>
        /// Returns a workspace object for the given context. This will be either a filesystem workspace, or an in-memory one for testing.
        /// </summary>
        IWorkspace Get();
    }
}
