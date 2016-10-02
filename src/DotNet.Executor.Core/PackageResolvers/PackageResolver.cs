namespace DotNet.Executor.Core.PackageResolvers
{
    using System;
    using System.IO;

    abstract class PackageResolver
    {
        protected DirectoryInfo PackagesFolder { get; set; }
        protected DirectoryInfo PackageFolder { get; set; }
        protected string Source { get; set; }

        abstract protected void Acquire();

        protected PackageResolver(DirectoryInfo packagesFolder, string source)
        {
            this.PackagesFolder = packagesFolder;
            this.Source = source;
        }

        public Package Resolve()
        {
            this.Acquire();

            if (this.PackageFolder == null)
                throw new Exception("PackageFolder property not set");
            
            if (this.PackagesFolder.FullName != this.PackageFolder.Parent.FullName)
                throw new Exception("Package folder not in specified packages folder");

            return new Package() { Directory = this.PackageFolder, EntryAssemblyFileName = this.PackageFolder.Name + ".dll" };
        }
    }
}