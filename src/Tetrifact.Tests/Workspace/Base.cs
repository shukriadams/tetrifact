﻿using Tetrifact.Core;

namespace Tetrifact.Tests.Workspace
{
    public class Base : FileSystemBase
    {
        protected IPackageCreateWorkspace Workspace;

        public Base()
        {
            Workspace = new Core.PackageCreateWorkspace(Settings, base.WorkspaceLogger, HashServiceHelper.Instance());
            Workspace.Initialize();
        }
    }
}
