using System;
using System.IO.Abstractions;
using Tetrifact.Core;

namespace Tetrifact.Tests.PackageList
{
    public class Base : TestBase, IDisposable
    {
        protected IPackageListService PackageList { get; set; }
        protected IPackageListCache PackageListCache { get; set; }
        protected new ITagsService TagService { get; set; }
        protected TestLogger<IPackageListService> PackageListLogger { get; set; }
        protected TestLogger<ITagsService> TagServiceLogger { get; set; }

        public Base()
        {
            ISettings settings = TestContext.Get<ISettings>();
            IFileSystem fileSystem = TestContext.Get<IFileSystem>();

            this.PackageListLogger = new TestLogger<IPackageListService>();
            this.TagServiceLogger = new TestLogger<ITagsService>();
            this.PackageListCache = TestContext.Get<IPackageListCache>();
            this.TagService = new Core.TagsService(settings, MemoryCacheHelper.GetInstance(), fileSystem, TagServiceLogger, PackageListCache);
            this.PackageList = new PackageListService(MemoryCacheHelper.GetInstance(), settings, new HashService(), TagService, fileSystem, this.PackageListLogger);
        }

        public void Dispose()
        {
            MemoryCacheHelper.GetInstance().Dispose();
        }
    }
}
