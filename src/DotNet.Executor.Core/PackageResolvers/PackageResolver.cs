namespace DotNet.Executor.Core.PackageResolvers
{
    using System.IO;

    abstract class PackageResolver
    {
        protected DirectoryInfo PackagesFolder { get; set; }
        protected string Source { get; set; }

        abstract protected Package Resolve();

        protected PackageResolver(DirectoryInfo packagesFolder, string source)
        {
            this.PackagesFolder = packagesFolder;
            this.Source = source;
        }
    }
}