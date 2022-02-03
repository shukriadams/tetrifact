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
            Bind<ITetriSettings>().To<TetriSettings>();
            Bind<IIndexReader>().To<TestIndexReader>();
            Bind<IRepositoryCleaner>().To<TestRepositoryCleaner>();
            Bind<IWorkspace>().To<TestingWorkspace>();
            Bind<IPackageList>().To<TestPackageList>();
            Bind<IFileSystem>().To<FileSystem>();
            Bind<IHashService>().To<HashService>();
            Bind<IPackageListCache>().To<TestPackageListCache>();
            Bind<ITagsService>().To<Core.TagsService>();
            Bind<IPackageCreate>().To<Core.PackageCreate>();
            Bind<ILogger<ITagsService>>().To<TestLogger<ITagsService>>();
            Bind<IThread>().To<ThreadDefault>();
            Bind<IPackagePrune>().To<Core.PackagePrune>();
            Bind<IPackageDiffService>().To<Core.PackageDiffService>();
            Bind<ILogger<PackagesController>>().To<TestLogger<PackagesController>>();
            Bind<ILogger<CleanController>>().To<TestLogger<CleanController>>();
            Bind<ILogger<FilesController>>().To<TestLogger<FilesController>>();
            Bind<ILogger<ArchivesController>>().To<TestLogger<ArchivesController>>();
            Bind<ILogger<TagsController>>().To<TestLogger<TagsController>>();
            Bind<ILogger<IWorkspace>>().To<TestLogger<IWorkspace>>();
            Bind<ILogger<IPackageCreate>>().To<TestLogger<IPackageCreate>>();
            Bind<ILogger<ITetriSettings>>().To<TestLogger<ITetriSettings>>();
            Bind<ILogger<IPackageDiffService>>().To<TestLogger<PackageDiffService>>();
        }
    }
}
