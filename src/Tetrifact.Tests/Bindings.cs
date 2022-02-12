﻿using Microsoft.Extensions.Logging;
using Ninject.Modules;
using System.IO.Abstractions;
using Tetrifact.Core;
using Tetrifact.Web;

namespace Tetrifact.Tests
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<ISettings>().To<Settings>();
            Bind<IIndexReader>().To<TestIndexReader>();
            Bind<IRepositoryCleaner>().To<TestRepositoryCleaner>();
            Bind<IPackageCreateWorkspace>().To<TestingWorkspace>();
            Bind<IPackageList>().To<TestPackageList>();
            Bind<IFileSystem>().To<FileSystem>();
            Bind<IHashService>().To<HashService>();
            Bind<IPackageListCache>().To<TestPackageListCache>();
            Bind<ITagsService>().To<Core.TagsService>();
            Bind<IPackageCreate>().To<Core.PackageCreate>();
            Bind<IThread>().To<ThreadDefault>();
            Bind<IPackagePrune>().To<Core.PackagePrune>();
            Bind<IPackageDiffService>().To<PackageDiffService>();
            Bind<IManagedFileSystem>().To<ThreadSafeFileSystem>();
            Bind<IArchiveService>().To<Core.ArchiveService>();
            Bind<ILogger<PackagesController>>().To<TestLogger<PackagesController>>();
            Bind<ILogger<CleanController>>().To<TestLogger<CleanController>>();
            Bind<ILogger<FilesController>>().To<TestLogger<FilesController>>();
            Bind<ILogger<ArchivesController>>().To<TestLogger<ArchivesController>>();
            Bind<ILogger<TagsController>>().To<TestLogger<TagsController>>();
            Bind<ILogger<IPackageCreateWorkspace>>().To<TestLogger<IPackageCreateWorkspace>>();
            Bind<ILogger<IPackageCreate>>().To<TestLogger<IPackageCreate>>();
            Bind<ILogger<ISettings>>().To<TestLogger<ISettings>>();
            Bind<ILogger<IPackageDiffService>>().To<TestLogger<IPackageDiffService>>();
            Bind<ILogger<ITagsService>>().To<TestLogger<ITagsService>>();
            Bind<ILogger<IArchiveService>>().To<TestLogger<IArchiveService>>();
            Bind<ILogger<IManagedFileSystem>>().To<TestLogger<ThreadSafeFileSystem>>();
        }
    }
}
