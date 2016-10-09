namespace DotNet.Globals.Core.PackageResolvers
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal abstract class PackageResolver
    {
        protected DirectoryInfo PackagesFolder { get; set; }
        protected DirectoryInfo PackageFolder { get; set; }
        protected string Source { get; set; }
        protected Options Options { get; set; }
        protected Package Package { get; set; } = new Package();

        abstract protected void Acquire();

        protected PackageResolver(DirectoryInfo packagesFolder, string source, Options options)
        {
            this.PackagesFolder = packagesFolder;
            this.Source = source;
            this.Options = options;

            this.Package.Options = options;
            this.Package.Source = source;
        }

        public Package Resolve()
        {
            this.Acquire();

            if (this.PackageFolder == null)
                throw new Exception("PackageFolder property not set");
            
            if (this.Package.EntryAssemblyFileName == null)
                throw new Exception("Entry assembly filename not set");

            if (this.PackagesFolder.FullName != this.PackageFolder.Parent.FullName)
                throw new Exception("Package folder not in specified packages folder");

            File.AppendAllText(Path.Combine(this.PackageFolder.FullName, "globals.json"), 
                JObject.Parse(JsonConvert.SerializeObject(this.Package)).ToString());

            return this.Package;
        }
    }
}