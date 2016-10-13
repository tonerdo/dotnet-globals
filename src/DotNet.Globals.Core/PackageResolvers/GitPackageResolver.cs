using System;
using System.IO;
using System.Linq;
using DotNet.Globals.Core.Utils;

namespace DotNet.Globals.Core.PackageResolvers
{
    internal class GitPackageResolver : FolderPackageResolver
    {
        public GitPackageResolver(DirectoryInfo packagesFolder, string source, Options options) : base(packagesFolder, source, options) { }

        protected override void Acquire()
        {
            var packageName = this.Source.Split('/').Last().Replace(".git", "");
            DirectoryInfo tempFolder = new DirectoryInfo(Path.GetTempPath())
                .CreateSubdirectory("dotnet-globals-" + Guid.NewGuid().ToString())
                .CreateSubdirectory(packageName);

            Reporter.Logger.LogInformation("Cloning git repository");
            bool clone = ProcessRunner.RunProcess("git", "clone", this.Source, tempFolder.FullName);
            if (!clone)
                throw new Exception("Unable to clone repository");

            Reporter.Logger.LogSuccess("Clone successful");
            this.Source = string.IsNullOrEmpty(this.Options.Folder) ? 
                tempFolder.FullName : Path.Combine(tempFolder.FullName, this.Options.Folder);

            base.Acquire();
        }
    }
}