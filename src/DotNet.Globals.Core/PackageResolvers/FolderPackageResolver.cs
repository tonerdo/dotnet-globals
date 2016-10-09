using System;
using System.IO;
using System.Linq;
using DotNet.Globals.Core.Utils;

namespace DotNet.Globals.Core.PackageResolvers
{
    internal class FolderPackageResolver : PackageResolver
    {

        public FolderPackageResolver(DirectoryInfo packagesFolder, string source, Options options) : base(packagesFolder, source, options) { }

        protected override void Acquire()
        {
            var sourceFolder = new DirectoryInfo(this.Source);
            FileInfo projectJson = sourceFolder.GetFiles().FirstOrDefault(f => f.Name == "project.json");

            if (projectJson == null)
                throw new Exception("No project.json found in source folder");

            bool restore = ProcessRunner.RunProcess("dotnet", "restore", projectJson.FullName);
            if (!restore)
                throw new Exception("Package restore for project failed");

            this.Package.EntryAssemblyFileName = $"{ProjectParser.GetEntryAssemblyName(projectJson)}.dll";
            string packageName = sourceFolder.Name;

            this.PackageFolder = this.PackagesFolder.GetDirectories().FirstOrDefault(d => d.Name == packageName);
            if (this.PackageFolder != null && this.PackageFolder.GetFiles().Select(f => f.Name).Contains("globals.json"))
                throw new Exception("A package with the same name already exists");
            else
                PackageRemover.RemoveFolder(this.PackageFolder);

            this.PackageFolder = this.PackagesFolder.CreateSubdirectory(packageName);
            bool build = ProcessRunner.RunProcess("dotnet", "build", projectJson.FullName,
                "-c Release", "-f netcoreapp1.0", "-o " + this.PackageFolder.FullName);
            if (!build)
                throw new Exception("Project compilation failed");
        }
    }
}