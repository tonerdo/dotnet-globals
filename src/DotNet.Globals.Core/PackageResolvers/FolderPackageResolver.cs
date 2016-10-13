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
            var globalsFolder = sourceFolder.Parent.Parent;

            FileInfo projectJson = sourceFolder.GetFiles().FirstOrDefault(f => f.Name.EndsWith("project.json"));
            if (projectJson == null)
                throw new Exception("No project.json found in source folder");

            Reporter.Logger.LogInformation("Restoring project dependencies");
            if (globalsFolder.GetFiles().FirstOrDefault(f => f.Name.EndsWith("global.json")) != null
                && !string.IsNullOrEmpty(this.Options.Folder))
            {
                bool restore = ProcessRunner.RunProcess("dotnet", "restore", globalsFolder.FullName);
                if (!restore)
                    throw new Exception("Package restore failed for project or one of its dependencies");
            }
            else
            {
                bool restore = ProcessRunner.RunProcess("dotnet", "restore", sourceFolder.FullName);
                if (!restore)
                    throw new Exception("Package restore for project failed");
            }

            Reporter.Logger.LogSuccess("Package restore for project successful");
            this.Package.EntryAssemblyFileName = $"{ProjectParser.GetEntryAssemblyName(projectJson)}.dll";
            string packageName = sourceFolder.Name;

            this.PackageFolder = this.PackagesFolder.GetDirectories().FirstOrDefault(d => d.Name == packageName);
            if (this.PackageFolder != null)
                if (this.PackageFolder.GetFiles().Select(f => f.Name).Contains("globals.json"))
                    throw new Exception("A package with the same name already exists");
                else
                    PackageRemover.RemoveFolder(this.PackageFolder);

            this.PackageFolder = this.PackagesFolder.CreateSubdirectory(packageName);
            Reporter.Logger.LogInformation("Building project");
            bool build = ProcessRunner.RunProcess("dotnet", "build", sourceFolder.FullName,
                "-c Release", "-f netcoreapp1.0", "-o " + this.PackageFolder.FullName);
            if (!build)
                throw new Exception("Project build failed");

            Reporter.Logger.LogSuccess("Project build successful");
        }
    }
}