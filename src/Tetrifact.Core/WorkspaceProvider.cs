namespace Tetrifact.Core
{
    public class WorkspaceProvider : IWorkspaceProvider
    {
        ITetriSettings _settings;

        public WorkspaceProvider(ITetriSettings settings)
        {
            _settings = settings;
        }

        public IWorkspace Get()
        {
            return new Workspace(_settings);
        }
    }
}
